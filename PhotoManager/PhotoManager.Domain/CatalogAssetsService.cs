using log4net;
using System.Reflection;

namespace PhotoManager.Domain;

public class CatalogAssetsService(
    IAssetRepository assetRepository,
    IAssetHashCalculatorService assetHashCalculatorService,
    IStorageService storageService,
    IUserConfigurationService userConfigurationService,
    IAssetsComparator assetsComparator)
    : ICatalogAssetsService
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    private bool _backupHasSameContent = true;
    private string _currentFolderPath = string.Empty;
    private readonly HashSet<string> _directories = [];

    public async Task CatalogAssetsAsync(CatalogChangeCallback? callback, CancellationToken? token = null)
    {
        // TODO: Improve Message for each event
        await Task.Run(() =>
        {
            int cataloguedAssetsBatchCount = 0;
            HashSet<string> visitedDirectories = [];

            try
            {
                HashSet<string> foldersPathToCatalog = GetFoldersPathToCatalog();

                foreach (string path in foldersPathToCatalog)
                {
                    // ThrowIfCancellationRequested should be in each if below ?
                    // token?.ThrowIfCancellationRequested(); rework all the cancellation
                    CatalogAssets(path, callback, ref cataloguedAssetsBatchCount, visitedDirectories, token);
                }

                _directories.UnionWith(visitedDirectories);

                if (!assetRepository.BackupExists() || !_backupHasSameContent)
                {
                    // TODO: Need to add ReasonEnum (or at least change the default value to match these cases)
                    callback?.Invoke(!assetRepository.BackupExists()
                        ? new CatalogChangeCallbackEventArgs { Message = "Creating catalog backup..." }
                        : new CatalogChangeCallbackEventArgs { Message = "Updating catalog backup..." });

                    assetRepository.WriteBackup();
                    // TODO: Need to add ReasonEnum (or at least change the default value to match these cases)
                    callback?.Invoke(new CatalogChangeCallbackEventArgs { Message = string.Empty });

                    _backupHasSameContent = true;
                }

                // TODO: Need to add ReasonEnum (or at least change the default value to match these cases)
                callback?.Invoke(new CatalogChangeCallbackEventArgs { Message = string.Empty });
            }
            catch (OperationCanceledException)
            {
                // If the catalog background process is cancelled,
                // there is a risk that it happens while saving the catalog files.
                // This could result in the files being damaged.
                // Therefore the application saves the files before the task is completely shut down.

                // TODO: Test if _currentFolderPath is good & SaveCatalog performed correctly
                Folder? currentFolder = assetRepository.GetFolderByPath(_currentFolderPath);
                assetRepository.SaveCatalog(currentFolder);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                // TODO: Need to add ReasonEnum (or at least change the default value to match these cases)
                callback?.Invoke(new CatalogChangeCallbackEventArgs { Exception = ex });
            }
            finally
            {
                // TODO: Need to add ReasonEnum (or at least change the default value to match these cases)
                callback?.Invoke(new CatalogChangeCallbackEventArgs { Message = string.Empty });
            }
        });
    }

    public Asset? CreateAsset(string directoryName, string fileName, bool isVideo = false)
    {
        Asset? asset = null;
        string? firstFrameVideoPath = null;

        if (isVideo && userConfigurationService.AssetSettings.AnalyseVideos)
        {
            firstFrameVideoPath = VideoHelper.GetFirstFramePath(
                directoryName,
                fileName,
                userConfigurationService.PathSettings.FirstFrameVideosPath,
                userConfigurationService.PathSettings.FfmpegPath); // Create an asset from the video file
        }

        if (assetRepository.IsAssetCatalogued(directoryName, fileName))
        {
            return asset;
        }

        string imagePath;

        // TODO: Rework this part
        if (!string.IsNullOrWhiteSpace(firstFrameVideoPath))
        {
            imagePath = firstFrameVideoPath;
            DirectoryInfo directoryInfo = new (imagePath);

            if (directoryName != directoryInfo.Parent?.FullName) // The video file is not in the same path than the asset created
            {
                return asset; // The asset is null because the target is not the video but the asset created previously
            }
        }
        else
        {
            imagePath = Path.Combine(directoryName, fileName);
        }

        byte[] imageBytes = storageService.GetFileBytes(imagePath);

        bool isHeic = imagePath.EndsWith(".heic", StringComparison.OrdinalIgnoreCase);
        bool isPng = imagePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
        bool isGif = imagePath.EndsWith(".gif", StringComparison.OrdinalIgnoreCase);

        if (!isHeic)
        {
            if (!storageService.IsValidGDIPlusImage(imageBytes))
            {
                return asset;
            } 
        }
        else
        {
            if (!storageService.IsValidHeic(imageBytes))
            {
                return asset;
            } 
        }

        ushort exifOrientation = (isPng || isGif)
            ? userConfigurationService.AssetSettings.DefaultExifOrientation
            : !isHeic
                ? storageService.GetExifOrientation(
                    imageBytes,
                    userConfigurationService.AssetSettings.DefaultExifOrientation,
                    userConfigurationService.AssetSettings.CorruptedImageOrientation) // GetExifOrientation is not handled by Gif and Png
                : storageService.GetHeicExifOrientation(imageBytes, userConfigurationService.AssetSettings.CorruptedImageOrientation);

        Rotation rotation = storageService.GetImageRotation(exifOrientation);

        bool assetCorrupted = false;
        bool assetRotated =  false;

        if (exifOrientation == userConfigurationService.AssetSettings.CorruptedImageOrientation)
        {
            assetCorrupted = true;
        }

        if (rotation != Rotation.Rotate0)
        {
            assetRotated = true;
        }

        BitmapImage originalImage = !isHeic
            ? storageService.LoadBitmapOriginalImage(imageBytes, rotation)
            : storageService.LoadBitmapHeicOriginalImage(imageBytes, rotation);

        double originalDecodeWidth = originalImage.PixelWidth;
        double originalDecodeHeight = originalImage.PixelHeight;
        double thumbnailDecodeWidth;
        double thumbnailDecodeHeight;
        double percentage;

        // If the original image is landscape
        if (originalDecodeWidth > originalDecodeHeight)
        {
            double thumbnailMaxWidth = userConfigurationService.AssetSettings.ThumbnailMaxWidth;
            thumbnailDecodeWidth = thumbnailMaxWidth;
            percentage = thumbnailMaxWidth * 100d / originalDecodeWidth;
            thumbnailDecodeHeight = percentage * originalDecodeHeight / 100d;
        }
        else // If the original image is portrait
        {
            double thumbnailMaxHeight = userConfigurationService.AssetSettings.ThumbnailMaxHeight;
            thumbnailDecodeHeight = thumbnailMaxHeight;
            percentage = thumbnailMaxHeight * 100d / originalDecodeHeight;
            thumbnailDecodeWidth = percentage * originalDecodeWidth / 100d;
        }

        BitmapImage thumbnailImage = !isHeic
            ? storageService.LoadBitmapThumbnailImage(imageBytes, rotation, Convert.ToInt32(thumbnailDecodeWidth), Convert.ToInt32(thumbnailDecodeHeight))
            : storageService.LoadBitmapHeicThumbnailImage(imageBytes, rotation, Convert.ToInt32(thumbnailDecodeWidth), Convert.ToInt32(thumbnailDecodeHeight));

        byte[] thumbnailBuffer = isPng
            ? storageService.GetPngBitmapImage(thumbnailImage)
            : isGif
                ? storageService.GetGifBitmapImage(thumbnailImage)
                : storageService.GetJpegBitmapImage(thumbnailImage);

        // directoryName comes from folder in assetRepository or CatalogExistingFolder that registers the folder if not in assetRepository
        Folder folder = assetRepository.GetFolderByPath(directoryName)!;

        asset = new Asset
        {
            FileName = Path.GetFileName(imagePath),
            FolderId = folder.FolderId,
            Folder = folder,
            FileSize = new FileInfo(imagePath).Length,
            PixelWidth = Convert.ToInt32(originalDecodeWidth),
            PixelHeight = Convert.ToInt32(originalDecodeHeight),
            ThumbnailPixelWidth = Convert.ToInt32(thumbnailDecodeWidth),
            ThumbnailPixelHeight = Convert.ToInt32(thumbnailDecodeHeight),
            ImageRotation = rotation,
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = assetHashCalculatorService.CalculateHash(imageBytes, imagePath),
            IsAssetCorrupted = assetCorrupted,
            AssetCorruptedMessage = assetCorrupted ? userConfigurationService.AssetSettings.AssetCorruptedMessage : null,
            IsAssetRotated = assetRotated,
            AssetRotatedMessage = assetRotated ? userConfigurationService.AssetSettings.AssetRotatedMessage : null,
        };

        assetRepository.AddAsset(asset, thumbnailBuffer);

        return asset;
    }

    #region private
    private HashSet<string> GetFoldersPathToCatalog()
    {
        string[] rootPaths = userConfigurationService.GetRootCatalogFolderPaths();

        foreach (string root in rootPaths)
        {
            if (!assetRepository.FolderExists(root))
            {
                assetRepository.AddFolder(root);
            }
        }

        return assetRepository.GetFoldersPath();
    }

    private void CatalogAssets(string directory, CatalogChangeCallback? callback, ref int cataloguedAssetsBatchCount, HashSet<string> visitedDirectories, CancellationToken? token = null)
    {
        _currentFolderPath = directory;
        int batchSize = userConfigurationService.AssetSettings.CatalogBatchSize;

        if (storageService.FolderExists(directory))
        {
            CatalogExistingFolder(directory, callback, ref cataloguedAssetsBatchCount, batchSize, visitedDirectories, token);
        }
        else if (!string.IsNullOrEmpty(directory) && !storageService.FolderExists(directory))
        {
            CatalogNonExistingFolder(directory, callback, ref cataloguedAssetsBatchCount, batchSize, token);
        }

        visitedDirectories.Add(directory);
    }

    private void CatalogExistingFolder(string directory, CatalogChangeCallback? callback, ref int cataloguedAssetsBatchCount, int batchSize, HashSet<string> visitedDirectories, CancellationToken? token = null)
    {
        Folder? folder;

        if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
        {
            return;
        }

        if (!assetRepository.FolderExists(directory))
        {
            folder = assetRepository.AddFolder(directory);

            callback?.Invoke(new CatalogChangeCallbackEventArgs
            {
                Folder = folder,
                Message = $"Folder {directory} added to catalog.",
                Reason = ReasonEnum.FolderCreated
            });
        }

        folder = assetRepository.GetFolderByPath(directory);

        // TODO: Need to add ReasonEnum (or at least change the default value to match these cases)
        callback?.Invoke(new CatalogChangeCallbackEventArgs
        {
            Folder = folder,
            Message = $"Inspecting folder {directory}."
        });

        List<Asset> cataloguedAssetsByPath = assetRepository.GetCataloguedAssetsByPath(directory);

        bool folderHasThumbnails = folder != null && assetRepository.FolderHasThumbnails(folder);

        if (!folderHasThumbnails)
        {
            foreach (Asset asset in cataloguedAssetsByPath)
            {
                asset.ImageData = LoadThumbnail(directory, asset.FileName, asset.ThumbnailPixelWidth, asset.ThumbnailPixelHeight);
            }
        }

        string[] filesName = storageService.GetFileNames(directory);

        CatalogNewAssets(directory, callback, ref cataloguedAssetsBatchCount, batchSize, filesName, cataloguedAssetsByPath, folderHasThumbnails, token);
        CatalogUpdatedAssets(directory, callback, ref cataloguedAssetsBatchCount, batchSize, cataloguedAssetsByPath, folderHasThumbnails, token);
        CatalogDeletedAssets(directory, callback, ref cataloguedAssetsBatchCount, batchSize, filesName, folder, cataloguedAssetsByPath, token);

        // TODO: SaveCatalog each time ? Better at the end or if cancelled, in the catch
        if (assetRepository.HasChanges() || !folderHasThumbnails)
        {
            assetRepository.SaveCatalog(folder);
        }

        if (cataloguedAssetsBatchCount >= batchSize || (!(!token?.IsCancellationRequested ?? true)))
        {
            return;
        }

        IEnumerable<DirectoryInfo> subdirectories = new DirectoryInfo(directory).EnumerateDirectories();

        foreach (DirectoryInfo subdirectory in subdirectories)
        {
            if (!_directories.Contains(subdirectory.FullName))
            {
                CatalogAssets(subdirectory.FullName, callback, ref cataloguedAssetsBatchCount, visitedDirectories, token);
            }
        }
    }

    private void CatalogNonExistingFolder(string directory, CatalogChangeCallback? callback, ref int cataloguedAssetsBatchCount, int batchSize, CancellationToken? token = null)
    {
        if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
        {
            return;
        }

        // If the folder doesn't exist anymore, the corresponding entry in the catalog and the thumbnails file are both deleted.
        // TODO: This should be tested in a new test method, in which the non existent folder is explicitly added to the catalog.
        Folder? folder = assetRepository.GetFolderByPath(directory);

        if (folder != null)
        {
            List<Asset> cataloguedAssetsByPath = assetRepository.GetCataloguedAssetsByPath(directory);

            foreach (Asset asset in cataloguedAssetsByPath)
            {
                // TODO: Only batchSize has been tested, it has to wait the IsCancellationRequested rework to full test the condition
                if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
                {
                    break;
                }

                assetRepository.DeleteAsset(directory, asset.FileName);
                _backupHasSameContent = false;
                cataloguedAssetsBatchCount++;

                callback?.Invoke(new CatalogChangeCallbackEventArgs
                {
                    Asset = asset,
                    CataloguedAssetsByPath = cataloguedAssetsByPath,
                    Message = $"Image {Path.Combine(directory, asset.FileName)} deleted from catalog.",
                    Reason = ReasonEnum.AssetDeleted
                });
            }

            cataloguedAssetsByPath = assetRepository.GetCataloguedAssetsByPath(directory);

            if (cataloguedAssetsByPath.Count == 0)
            {
                assetRepository.DeleteFolder(folder);
                _directories.Remove(folder.Path);

                callback?.Invoke(new CatalogChangeCallbackEventArgs
                {
                    Folder = folder,
                    Message = $"Folder {directory} deleted from catalog.",
                    Reason = ReasonEnum.FolderDeleted
                });
            }

            if (assetRepository.HasChanges())
            {
                assetRepository.SaveCatalog(folder);
            }
        }
    }

    private void CatalogNewAssets(string directory, CatalogChangeCallback? callback, ref int cataloguedAssetsBatchCount, int batchSize, string[] fileNames, List<Asset> cataloguedAssetsByPath, bool folderHasThumbnails, CancellationToken? token = null)
    {
        (string[] imageNames, string[] videoNames) = assetsComparator.GetImageAndVideoNames(fileNames);

        string[] newImageFileNames = assetsComparator.GetNewFileNames(imageNames, cataloguedAssetsByPath);
        string[] newVideoFileNames = assetsComparator.GetNewFileNames(videoNames, cataloguedAssetsByPath);

        CreateAssets(newImageFileNames, false, directory, callback, ref cataloguedAssetsBatchCount, batchSize, cataloguedAssetsByPath, folderHasThumbnails, token);

        if (userConfigurationService.AssetSettings.AnalyseVideos)
        {
            CreateAssets(newVideoFileNames, true, directory, callback, ref cataloguedAssetsBatchCount, batchSize, cataloguedAssetsByPath, folderHasThumbnails, token);
        }
    }

    private void CreateAssets(string[] fileNames, bool isAssetVideo, string directory, CatalogChangeCallback? callback, ref int cataloguedAssetsBatchCount, int batchSize, List<Asset> cataloguedAssetsByPath, bool folderHasThumbnails, CancellationToken? token = null)
    {
        foreach (string fileName in fileNames)
        {
            if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
            {
                break;
            }

            Asset? newAsset = CreateAsset(directory, fileName, isAssetVideo);

            if (newAsset == null)
            {
                continue;
            }

            newAsset.ImageData = LoadThumbnail(directory, newAsset.FileName, newAsset.ThumbnailPixelWidth, newAsset.ThumbnailPixelHeight);

            // if (!folderHasThumbnails)
            // {
                cataloguedAssetsByPath.Add(newAsset);
            // }

            // TODO: Rework the way cataloguedAssetsByPath is handled (bad practice to modify it like this above) + need to rework how to update file information
            // cataloguedAssetsByPath = _assetRepository.GetCataloguedAssetsByPath(directory);

            // TODO: Reorder each CatalogChangeCallbackEventArgs to match with the class (same for each UT)
            callback?.Invoke(new CatalogChangeCallbackEventArgs
            {
                Asset = newAsset,
                CataloguedAssetsByPath = cataloguedAssetsByPath,
                Message = $"Image {Path.Combine(directory, newAsset.FileName)} added to catalog.",
                Reason = ReasonEnum.AssetCreated
            });

            _backupHasSameContent = false;
            cataloguedAssetsBatchCount++;
        }
    }

    private void CatalogUpdatedAssets(string directory, CatalogChangeCallback? callback, ref int cataloguedAssetsBatchCount, int batchSize, List<Asset> cataloguedAssetsByPath, bool folderHasThumbnails, CancellationToken? token = null)
    {
        string[] updatedFileNames = assetsComparator.GetUpdatedFileNames(cataloguedAssetsByPath); // TODO: Should not depend on it to have file info for each files -> break content in separate parts
        //Folder folder = _assetRepository.GetFolderByPath(directory);

        foreach (string fileName in updatedFileNames)
        {
            // TODO: Only batchSize has been tested, it has to wait the IsCancellationRequested rework to full test the condition
            if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
            {
                break;
            }

            assetRepository.DeleteAsset(directory, fileName);
            _backupHasSameContent = false;
            string fullPath = Path.Combine(directory, fileName);

            if (storageService.FileExists(fullPath))
            {
                Asset? updatedAsset = CreateAsset(directory, fileName);

                if (updatedAsset == null)
                {
                    continue;
                }

                // TODO: Move from here and split _assetsComparator.GetUpdatedFileNames usage above !!
                storageService.LoadFileInformation(updatedAsset);

                updatedAsset.ImageData = LoadThumbnail(directory, fileName, updatedAsset.ThumbnailPixelWidth, updatedAsset.ThumbnailPixelHeight);

                // TODO: Check this condition about folderHasThumbnails (seems to be useless here because already added above)
                // if (!folderHasThumbnails)
                // {
                //     cataloguedAssetsByPath.Add(updatedAsset);
                // }

                cataloguedAssetsByPath = assetRepository.GetCataloguedAssetsByPath(directory);

                callback?.Invoke(new CatalogChangeCallbackEventArgs
                {
                    Asset = updatedAsset,
                    CataloguedAssetsByPath = cataloguedAssetsByPath,
                    Message = $"Image {fullPath} updated in catalog.",
                    Reason = ReasonEnum.AssetUpdated
                });

                cataloguedAssetsBatchCount++;
            }
        }
    }

    private void CatalogDeletedAssets(string directory, CatalogChangeCallback? callback, ref int cataloguedAssetsBatchCount, int batchSize, string[] fileNames, Folder folder, List<Asset> cataloguedAssetsByPath, CancellationToken? token = null)
    {
        string[] deletedFileNames = assetsComparator.GetDeletedFileNames(fileNames, cataloguedAssetsByPath);

        foreach (string fileName in deletedFileNames)
        {
            // TODO: Only batchSize has been tested, it has to wait the IsCancellationRequested rework to full test the condition
            if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
            {
                break;
            }

            assetRepository.DeleteAsset(directory, fileName);

            Asset? deletedAsset = cataloguedAssetsByPath.FirstOrDefault(x => x.FileName == fileName && x.FolderId == folder.FolderId);
            cataloguedAssetsByPath.Remove(deletedAsset);

            callback?.Invoke(new CatalogChangeCallbackEventArgs
            {
                Asset = deletedAsset,
                CataloguedAssetsByPath = cataloguedAssetsByPath,
                Message = $"Image {Path.Combine(directory, fileName)} deleted from catalog.",
                Reason = ReasonEnum.AssetDeleted
            });

            _backupHasSameContent = false;
            cataloguedAssetsBatchCount++;
        }
    }

    private BitmapImage? LoadThumbnail(string directoryName, string fileName, int width, int height)
    {
        BitmapImage? thumbnailImage = null;

        if (assetRepository.ContainsThumbnail(directoryName, fileName))
        {
            thumbnailImage = assetRepository.LoadThumbnail(directoryName, fileName, width, height);
        }

        return thumbnailImage;
    }
    #endregion
}
