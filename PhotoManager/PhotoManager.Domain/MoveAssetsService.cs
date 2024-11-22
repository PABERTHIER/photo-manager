using log4net;
using System.Reflection;

namespace PhotoManager.Domain;

public class MoveAssetsService(
    IAssetRepository assetRepository,
    IStorageService storageService,
    IAssetCreationService assetCreationService)
    : IMoveAssetsService
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public bool MoveAssets(Asset[] assets, Folder destinationFolder, bool preserveOriginalFiles)
    {
        if (destinationFolder == null)
        {
            throw new ArgumentNullException(nameof(destinationFolder), "destinationFolder cannot be null.");
        }

        ValidateParameters(assets);

        bool result = false;
        Folder? folder = assetRepository.GetFolderByPath(destinationFolder.Path); // If the folder is null, it means it is not present in the catalog

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
                destinationFolder = assetRepository.AddFolder(destinationFolder.Path);
                isDestinationFolderInCatalog = true;

            }

            assetCreationService.CreateAsset(destinationFolder.Path, asset.FileName);

            if (result && !preserveOriginalFiles && !destinationFolder.IsSameDirectory(assets[0].Folder))
            {
                DeleteAsset(asset);
            }
        }

        if (result)
        {
            assetRepository.UpdateTargetPathToRecent(destinationFolder);
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
            Log.Error($"Cannot copy '{sourceFilePath}' because the destination path is null or empty.");
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

            if (!storageService.FileExists(asset.Folder, asset))
            {
                throw new FileNotFoundException($"File does not exist: '{asset.FullPath}'.");
            }
        }
    }

    private void DeleteAsset(Asset asset)
    {
        _ = assetRepository.DeleteAsset(asset.Folder.Path, asset.FileName);
        storageService.DeleteFile(asset.Folder.Path, asset.FileName);
    }

    private bool Copy(string sourceFilePath, string destinationFilePath)
    {
        try
        {
            if (storageService.FileExists(destinationFilePath))
            {
                Log.Error($"Cannot copy '{sourceFilePath}' into '{destinationFilePath}' because the file already exists in the destination.");
                return storageService.FileExists(sourceFilePath);
            }

            string destinationFolderPath = new FileInfo(destinationFilePath).Directory!.FullName;

            if (!Directory.Exists(destinationFolderPath))
            {
                storageService.CreateDirectory(destinationFolderPath);
            }

            File.Copy(sourceFilePath, destinationFilePath);

            return storageService.FileExists(sourceFilePath) && storageService.FileExists(destinationFilePath);
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException($"File does not exist: '{sourceFilePath}'.");
        }
        catch (DirectoryNotFoundException)
        {
            throw new DirectoryNotFoundException($"Could not find a part of the path '{sourceFilePath}'.");
        }
        catch (IOException)
        {
            throw new IOException($"The target file '{destinationFilePath}' is a directory, not a file.");
        }
        catch (ArgumentNullException)
        {
            throw new ArgumentNullException(nameof(sourceFilePath), "Value cannot be null.");
        }
        catch (ArgumentException)
        {
            throw new ArgumentException("The value cannot be an empty string.", nameof(sourceFilePath));
        }
        catch (Exception ex)
        {
            Log.Error($"Cannot copy '{sourceFilePath}' into '{destinationFilePath}' due to insufficient permissions, disk space issues, or file locking problems, Message: {ex.Message}");
            return false;
        }
    }

    private void SaveCatalog(Folder folder)
    {
        assetRepository.SaveCatalog(folder);
    }
}
