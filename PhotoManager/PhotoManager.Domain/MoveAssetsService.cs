using Microsoft.Extensions.Logging;

namespace PhotoManager.Domain;

public class MoveAssetsService(
    IAssetRepository assetRepository,
    IFileOperationsService fileOperationsService,
    IAssetCreationService assetCreationService,
    ILogger<MoveAssetsService> logger)
    : IMoveAssetsService
{
    public bool MoveAssets(Asset[] assets, Folder destinationFolder, bool preserveOriginalFiles)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (destinationFolder == null)
        {
            logger.LogError("Cannot move assets because the destination folder is null.");
            throw new ArgumentNullException(nameof(destinationFolder), "destinationFolder cannot be null.");
        }

        ValidateParameters(assets);

        bool result = false;
        // If the folder is null, it means it is not present in the catalog
        Folder? folder = assetRepository.GetFolderByPath(destinationFolder.Path);

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
            logger.LogError("Cannot copy '{sourceFilePath}' because the destination path is null or empty.",
                sourceFilePath);
            return false;
        }

        return Copy(sourceFilePath, destinationFilePath);
    }

    private void ValidateParameters(Asset[] assets)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (assets == null || assets.Length == 0)
        {
            logger.LogError("Cannot validate assets because the assets array is null or empty.");
            throw new ArgumentNullException(nameof(assets), "assets cannot be null or empty.");
        }

        foreach (Asset asset in assets)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (asset == null)
            {
                logger.LogError("Cannot validate asset because one of the assets in the array is null.");
                throw new ArgumentNullException(nameof(asset), "asset cannot be null.");
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (asset.Folder == null)
            {
                logger.LogError($"Cannot validate asset '{asset.FileName}' because the folder is null.");
                throw new ArgumentNullException(nameof(asset.Folder), "asset.Folder cannot be null.");
            }

            if (!fileOperationsService.FileExists(asset.Folder, asset))
            {
                logger.LogError(
                    "Cannot validate asset '{assetFileName}' because the file does not exist at '{assetFullPath}'.",
                    asset.FileName, asset.FullPath);
                throw new FileNotFoundException($"File does not exist: '{asset.FullPath}'.");
            }
        }
    }

    private void DeleteAsset(Asset asset)
    {
        _ = assetRepository.DeleteAsset(asset.Folder.Path, asset.FileName);
        fileOperationsService.DeleteFile(asset.Folder.Path, asset.FileName);
    }

    private bool Copy(string sourceFilePath, string destinationFilePath)
    {
        try
        {
            if (string.Equals(sourceFilePath, destinationFilePath, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogError(
                    "Cannot copy '{sourceFilePath}' into '{destinationFilePath}' because the source and destination are the same.",
                    sourceFilePath,
                    destinationFilePath);
                return fileOperationsService.FileExists(sourceFilePath);
            }

            if (fileOperationsService.FileExists(destinationFilePath))
            {
                logger.LogError(
                    "Cannot copy '{sourceFilePath}' into '{destinationFilePath}' because the file already exists in the destination.",
                    sourceFilePath,
                    destinationFilePath);
                return fileOperationsService.FileExists(sourceFilePath);
            }

            string destinationFolderPath = new FileInfo(destinationFilePath).Directory!.FullName;

            if (!Directory.Exists(destinationFolderPath))
            {
                fileOperationsService.CreateDirectory(destinationFolderPath);
            }

            File.Copy(sourceFilePath, destinationFilePath);

            return fileOperationsService.FileExists(sourceFilePath) &&
                   fileOperationsService.FileExists(destinationFilePath);
        }
        catch (FileNotFoundException)
        {
            logger.LogError(
                "Cannot copy '{sourceFilePath}' into '{destinationFilePath}' because the source file does not exist.",
                sourceFilePath,
                destinationFilePath);
            throw new FileNotFoundException($"File does not exist: '{sourceFilePath}'.");
        }
        catch (DirectoryNotFoundException)
        {
            logger.LogError(
                "Cannot copy '{sourceFilePath}' into '{destinationFilePath}' because the source directory does not exist.",
                sourceFilePath,
                destinationFilePath);
            throw new DirectoryNotFoundException($"Could not find a part of the path '{sourceFilePath}'.");
        }
        catch (IOException)
        {
            logger.LogError(
                "Cannot copy '{sourceFilePath}' into '{destinationFilePath}' because the target is a directory, not a file.",
                sourceFilePath,
                destinationFilePath);
            throw new IOException($"The target file '{destinationFilePath}' is a directory, not a file.");
        }
        catch (ArgumentNullException)
        {
            logger.LogError(
                "Cannot copy '(null)' into '{destinationFilePath}' because the source path is null.",
                destinationFilePath);
            throw new ArgumentNullException(nameof(sourceFilePath), "Value cannot be null.");
        }
        catch (ArgumentException)
        {
            logger.LogError(
                "Cannot copy empty string into '{destinationFilePath}' because the source path is empty.",
                destinationFilePath);
            throw new ArgumentException("The value cannot be an empty string.", nameof(sourceFilePath));
        }
        catch (Exception ex)
        {
            logger.LogError(
                "Cannot copy '{sourceFilePath}' into '{destinationFilePath}' due to insufficient permissions, disk space issues, or file locking problems, Message: {ex.Message}",
                sourceFilePath,
                destinationFilePath,
                ex.Message);
            return false;
        }
    }

    private void SaveCatalog(Folder folder)
    {
        assetRepository.SaveCatalog(folder);
    }
}
