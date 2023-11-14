namespace PhotoManager.Domain;

public class MoveAssetsService : IMoveAssetsService
{
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
        if (destinationFolder == null)
        {
            throw new ArgumentNullException(nameof(destinationFolder), "destinationFolder cannot be null.");
        }

        ValidateParameters(assets);

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

            result = Copy(sourceFilePath, destinationFilePath);

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

            if (result && !preserveOriginalFiles && !destinationFolder.IsSameDirectory(assets[0].Folder))
            {
                DeleteAsset(asset);
            }
        }

        if (result)
        {
            _assetRepository.UpdateTargetPathToRecent(destinationFolder);
            SaveCatalog(destinationFolder);
        }

        return result;
    }

    public void DeleteAssets(Asset[] assets)
    {
        ValidateParameters(assets);

        foreach (Asset asset in assets)
        {
            DeleteAsset(asset);
        }

        SaveCatalog(assets[0].Folder);
    }

    public bool CopyAsset(string sourceFilePath, string destinationFilePath)
    {
        if (string.IsNullOrWhiteSpace(destinationFilePath))
        {
            // TODO: Log the destinationFolderPath is null or empty
            return false;
        }

        return Copy(sourceFilePath, destinationFilePath);
    }

    private void ValidateParameters(Asset[] assets)
    {
        if (assets == null || assets.Length == 0)
        {
            throw new ArgumentNullException(nameof(assets), "assets cannot be null or empty.");
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

            if (!_storageService.FileExists(asset, asset.Folder))
            {
                throw new FileNotFoundException($"File does not exist: '{asset.FullPath}'.");
            }
        }
    }

    private void DeleteAsset(Asset asset)
    {
        _assetRepository.DeleteAsset(asset.Folder.Path, asset.FileName);
        _storageService.DeleteFile(asset.Folder.Path, asset.FileName);
    }

    private bool Copy(string sourceFilePath, string destinationFilePath)
    {
        try
        {
            if (_storageService.FileExists(destinationFilePath))
            {
                // TODO: Log the file already exists in the destination
                return _storageService.FileExists(sourceFilePath);
            }

            string destinationFolderPath = new FileInfo(destinationFilePath).Directory!.FullName;

            if (!Directory.Exists(destinationFolderPath))
            {
                _storageService.CreateDirectory(destinationFolderPath);
            }

            File.Copy(sourceFilePath, destinationFilePath);

            return _storageService.FileExists(sourceFilePath) && _storageService.FileExists(destinationFilePath);
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException($"File does not exist: '{sourceFilePath}'.");
        }
        catch (DirectoryNotFoundException)
        {
            throw new DirectoryNotFoundException($"Could not find a part of the path '{sourceFilePath}'.");
        }
        catch (ArgumentNullException)
        {
            throw new ArgumentNullException(nameof(sourceFilePath), "Value cannot be null.");
        }
        catch (ArgumentException)
        {
            throw new ArgumentException("The value cannot be an empty string.", nameof(sourceFilePath));
        }
        catch
        {
            // TODO: Log the file could not be copied: insufficient permissions, disk space issues, or file locking problems ?
            return false;
        }
    }

    private void SaveCatalog(Folder folder)
    {
        _assetRepository.SaveCatalog(folder);
    }
}
