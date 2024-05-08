using log4net;
using System.Reflection;

namespace PhotoManager.Domain;

public class CatalogAssetsService : ICatalogAssetsService
{
    private readonly IAssetRepository _assetRepository;
    private readonly IAssetHashCalculatorService _assetHashCalculatorService;
    private readonly IStorageService _storageService;
    private readonly IUserConfigurationService _userConfigurationService;
    private readonly IDirectoryComparer _directoryComparer;

    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private bool _backupHasSameContent;
    private string _currentFolderPath;
    private HashSet<string> _directories;

    public CatalogAssetsService(
        IAssetRepository assetRepository,
        IAssetHashCalculatorService assetHashCalculatorService,
        IStorageService storageService,
        IUserConfigurationService userConfigurationService,
        IDirectoryComparer directoryComparer)
    {
        _assetRepository = assetRepository;
        _assetHashCalculatorService = assetHashCalculatorService;
        _storageService = storageService;
        _userConfigurationService = userConfigurationService;
        _directoryComparer = directoryComparer;

        _backupHasSameContent = true;
        _currentFolderPath = string.Empty;
        _directories = [];
    }

    public async Task CatalogAssetsAsync(CatalogChangeCallback? callback, CancellationToken? token = null)
    {
        // TODO: Improve Message for each event
        await Task.Run(() =>
        {
            int cataloguedAssetsBatchCount = 0;
            HashSet<string> visitedDirectories = [];

            try
            {
                Folder[] foldersToCatalog = GetFoldersToCatalog();

                foreach (Folder folder in foldersToCatalog)
                {
                    // ThrowIfCancellationRequested should be in each if below ?
                    // token?.ThrowIfCancellationRequested(); rework all the cancellation
                    CatalogAssets(folder.Path, callback, ref cataloguedAssetsBatchCount, visitedDirectories, token);
                }

                _directories.UnionWith(visitedDirectories);

                if (!_assetRepository.BackupExists() || !_backupHasSameContent)
                {
                    // TODO: Need to add ReasonEnum (or at least change the default value to match these cases)
                    callback?.Invoke(!_assetRepository.BackupExists()
                        ? new CatalogChangeCallbackEventArgs { Message = "Creating catalog backup..." }
                        : new CatalogChangeCallbackEventArgs { Message = "Updating catalog backup..." });

                    _assetRepository.WriteBackup();
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
                Folder? currentFolder = _assetRepository.GetFolderByPath(_currentFolderPath);
                _assetRepository.SaveCatalog(currentFolder);
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

        if (isVideo && _userConfigurationService.AssetSettings.AnalyseVideos)
        {
            firstFrameVideoPath = VideoHelper.GetFirstFramePath(
                directoryName,
                fileName,
                _userConfigurationService.PathSettings.FirstFrameVideosPath,
                _userConfigurationService.PathSettings.FfmpegPath); // Create an asset from the video file
        }

        if (!_assetRepository.IsAssetCatalogued(directoryName, fileName))
        {
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

            byte[] imageBytes = _storageService.GetFileBytes(imagePath);

            bool isHeic = imagePath.EndsWith(".heic", StringComparison.OrdinalIgnoreCase);

            if (isHeic)
            {
                return CreateAssetFromHeic(imageBytes, imagePath, directoryName);
            }

            if (!_storageService.IsValidGDIPlusImage(imageBytes))
            {
                return asset;
            }

            bool isPng = imagePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
            bool isGif = imagePath.EndsWith(".gif", StringComparison.OrdinalIgnoreCase);

            ushort exifOrientation = (isPng || isGif)
                ? _userConfigurationService.AssetSettings.DefaultExifOrientation
                : _storageService.GetExifOrientation(
                    imageBytes,
                    _userConfigurationService.AssetSettings.DefaultExifOrientation,
                    _userConfigurationService.AssetSettings.CorruptedImageOrientation); // GetExifOrientation is not handled by Gif and Png

            Rotation rotation = _storageService.GetImageRotation(exifOrientation);

            bool assetCorrupted = false;
            bool assetRotated =  false;

            if (exifOrientation == _userConfigurationService.AssetSettings.CorruptedImageOrientation)
            {
                assetCorrupted = true;
            }

            if (rotation != Rotation.Rotate0)
            {
                assetRotated = true;
            }

            BitmapImage originalImage = _storageService.LoadBitmapOriginalImage(imageBytes, rotation);

            double originalDecodeWidth = originalImage.PixelWidth;
            double originalDecodeHeight = originalImage.PixelHeight;
            double thumbnailDecodeWidth;
            double thumbnailDecodeHeight;
            double percentage;

            // If the original image is landscape
            if (originalDecodeWidth > originalDecodeHeight)
            {
                double thumbnailMaxWidth = _userConfigurationService.AssetSettings.ThumbnailMaxWidth;
                thumbnailDecodeWidth = thumbnailMaxWidth;
                percentage = (thumbnailMaxWidth * 100d / originalDecodeWidth);
                thumbnailDecodeHeight = (percentage * originalDecodeHeight) / 100d;
            }
            else // If the original image is portrait
            {
                double thumbnailMaxHeight = _userConfigurationService.AssetSettings.ThumbnailMaxHeight;
                thumbnailDecodeHeight = thumbnailMaxHeight;
                percentage = (thumbnailMaxHeight * 100d / originalDecodeHeight);
                thumbnailDecodeWidth = (percentage * originalDecodeWidth) / 100d;
            }
            
            BitmapImage thumbnailImage = _storageService.LoadBitmapThumbnailImage(
                imageBytes,
                rotation,
                Convert.ToInt32(thumbnailDecodeWidth),
                Convert.ToInt32(thumbnailDecodeHeight));

            byte[] thumbnailBuffer = isPng ? _storageService.GetPngBitmapImage(thumbnailImage) :
                (isGif ? _storageService.GetGifBitmapImage(thumbnailImage) : _storageService.GetJpegBitmapImage(thumbnailImage));

            Folder folder = _assetRepository.GetFolderByPath(directoryName);

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
                Hash = _assetHashCalculatorService.CalculateHash(imageBytes, imagePath),
                IsAssetCorrupted = assetCorrupted,
                AssetCorruptedMessage = assetCorrupted ? _userConfigurationService.AssetSettings.AssetCorruptedMessage : null,
                IsAssetRotated = assetRotated,
                AssetRotatedMessage = assetRotated ? _userConfigurationService.AssetSettings.AssetRotatedMessage : null,
            };

            _assetRepository.AddAsset(asset, thumbnailBuffer);
        }

        return asset;
    }

    #region private
    private Folder[] GetFoldersToCatalog()
    {
        string[] rootPaths = _userConfigurationService.GetRootCatalogFolderPaths();

        foreach (string root in rootPaths)
        {
            if (!_assetRepository.FolderExists(root))
            {
                _assetRepository.AddFolder(root);
            }
        }

        return _assetRepository.GetFolders();
    }

    private void CatalogAssets(string directory, CatalogChangeCallback? callback, ref int cataloguedAssetsBatchCount, HashSet<string> visitedDirectories, CancellationToken? token = null)
    {
        if (visitedDirectories.Contains(directory))
        {
            return;
        }

        _currentFolderPath = directory;
        int batchSize = _userConfigurationService.AssetSettings.CatalogBatchSize;

        if (_storageService.FolderExists(directory))
        {
            CatalogExistingFolder(directory, callback, ref cataloguedAssetsBatchCount, batchSize, visitedDirectories, token);
        }
        else if (!string.IsNullOrEmpty(directory) && !_storageService.FolderExists(directory))
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

        if (!_assetRepository.FolderExists(directory))
        {
            folder = _assetRepository.AddFolder(directory);

            callback?.Invoke(new CatalogChangeCallbackEventArgs
            {
                Folder = folder,
                Message = $"Folder {directory} added to catalog.",
                Reason = ReasonEnum.FolderCreated
            });
        }

        folder = _assetRepository.GetFolderByPath(directory);

        // TODO: Need to add ReasonEnum (or at least change the default value to match these cases)
        callback?.Invoke(new CatalogChangeCallbackEventArgs
        {
            Folder = folder,
            Message = $"Inspecting folder {directory}."
        });

        List<Asset> cataloguedAssetsByPath = _assetRepository.GetCataloguedAssetsByPath(directory);

        bool folderHasThumbnails = folder != null && _assetRepository.FolderHasThumbnails(folder);

        if (!folderHasThumbnails)
        {
            foreach (Asset asset in cataloguedAssetsByPath)
            {
                asset.ImageData = LoadThumbnail(directory, asset.FileName, asset.ThumbnailPixelWidth, asset.ThumbnailPixelHeight);
            }
        }

        string[] filesName = _storageService.GetFileNames(directory);

        CatalogNewAssets(directory, callback, ref cataloguedAssetsBatchCount, batchSize, filesName, cataloguedAssetsByPath, folderHasThumbnails, token);
        CatalogUpdatedAssets(directory, callback, ref cataloguedAssetsBatchCount, batchSize, cataloguedAssetsByPath, folderHasThumbnails, token);
        CatalogDeletedAssets(directory, callback, ref cataloguedAssetsBatchCount, batchSize, filesName, folder, cataloguedAssetsByPath, token);

        // TODO: SaveCatalog each time ? Better at the end or if cancelled, in the catch
        if (_assetRepository.HasChanges() || !folderHasThumbnails)
        {
            _assetRepository.SaveCatalog(folder);
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
        Folder? folder = _assetRepository.GetFolderByPath(directory);

        if (folder != null)
        {
            List<Asset> cataloguedAssetsByPath = _assetRepository.GetCataloguedAssetsByPath(directory);

            foreach (Asset asset in cataloguedAssetsByPath)
            {
                // TODO: Only batchSize has been tested, it has to wait the IsCancellationRequested rework to full test the condition
                if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
                {
                    break;
                }

                _assetRepository.DeleteAsset(directory, asset.FileName);
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

            cataloguedAssetsByPath = _assetRepository.GetCataloguedAssetsByPath(directory);

            if (cataloguedAssetsByPath.Count == 0)
            {
                _assetRepository.DeleteFolder(folder);
                _directories.Remove(folder.Path);

                callback?.Invoke(new CatalogChangeCallbackEventArgs
                {
                    Folder = folder,
                    Message = $"Folder {directory} deleted from catalog.",
                    Reason = ReasonEnum.FolderDeleted
                });
            }

            if (_assetRepository.HasChanges())
            {
                _assetRepository.SaveCatalog(folder);
            }
        }
    }

    private void CatalogNewAssets(string directory, CatalogChangeCallback? callback, ref int cataloguedAssetsBatchCount, int batchSize, string[] fileNames, List<Asset> cataloguedAssetsByPath, bool folderHasThumbnails, CancellationToken? token = null)
    {
        string[] imageNames;
        string[] videoNames;

        (imageNames, videoNames) = _directoryComparer.GetImageAndVideoNames(fileNames);

        string[] newImageFileNames = _directoryComparer.GetNewFileNames(imageNames, cataloguedAssetsByPath);
        string[] newVideoFileNames = _directoryComparer.GetNewFileNames(videoNames, cataloguedAssetsByPath);

        CreateAssets(newImageFileNames, false, directory, callback, ref cataloguedAssetsBatchCount, batchSize, cataloguedAssetsByPath, folderHasThumbnails, token);

        if (_userConfigurationService.AssetSettings.AnalyseVideos)
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

            // TODO: Refacto the way cataloguedAssetsByPath is handled (bad practice to modify it like this above) + need to rework how to update file informations
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

    // TODO: Facto with initial method
    private Asset? CreateAssetFromHeic(byte[] imageBytes, string imagePath, string directoryName)
    {
        Asset? asset = null;

        if (!_storageService.IsValidHeic(imageBytes))
        {
            return asset;
        }

        ushort exifOrientation = _storageService.GetHeicExifOrientation(imageBytes, _userConfigurationService.AssetSettings.CorruptedImageOrientation);
        Rotation rotation = _storageService.GetImageRotation(exifOrientation);

        bool assetCorrupted = false;
        bool assetRotated = false;

        if (exifOrientation == _userConfigurationService.AssetSettings.CorruptedImageOrientation)
        {
            assetCorrupted = true;
        }

        // MagickImage always returns "TopLeft", it is not able to detect the right orientation for a heic file -_-
        if (rotation != Rotation.Rotate0)
        {
            assetRotated = true;
        }

        BitmapImage originalImage = _storageService.LoadBitmapHeicOriginalImage(imageBytes, rotation);

        double originalDecodeWidth = originalImage.PixelWidth;
        double originalDecodeHeight = originalImage.PixelHeight;
        double thumbnailDecodeWidth;
        double thumbnailDecodeHeight;
        double percentage;

        // If the original image is landscape
        if (originalDecodeWidth > originalDecodeHeight)
        {
            double thumbnailMaxWidth = _userConfigurationService.AssetSettings.ThumbnailMaxWidth;
            thumbnailDecodeWidth = thumbnailMaxWidth;
            percentage = (thumbnailMaxWidth * 100d / originalDecodeWidth);
            thumbnailDecodeHeight = (percentage * originalDecodeHeight) / 100d;
        }
        else // If the original image is portrait
        {
            double thumbnailMaxHeight = _userConfigurationService.AssetSettings.ThumbnailMaxHeight;
            thumbnailDecodeHeight = thumbnailMaxHeight;
            percentage = (thumbnailMaxHeight * 100d / originalDecodeHeight);
            thumbnailDecodeWidth = (percentage * originalDecodeWidth) / 100d;
        }

        BitmapImage thumbnailImage = _storageService.LoadBitmapHeicThumbnailImage(imageBytes,
            rotation,
            Convert.ToInt32(thumbnailDecodeWidth),
            Convert.ToInt32(thumbnailDecodeHeight));

        byte[] thumbnailBuffer = _storageService.GetJpegBitmapImage(thumbnailImage);

        Folder folder = _assetRepository.GetFolderByPath(directoryName);

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
            Hash = _assetHashCalculatorService.CalculateHash(imageBytes, imagePath),
            IsAssetCorrupted = assetCorrupted,
            AssetCorruptedMessage = assetCorrupted ? _userConfigurationService.AssetSettings.AssetCorruptedMessage : null,
            IsAssetRotated = assetRotated,
            AssetRotatedMessage = assetRotated ? _userConfigurationService.AssetSettings.AssetRotatedMessage : null,
        };

        _assetRepository.AddAsset(asset, thumbnailBuffer);

        return asset;
    }

    private void CatalogUpdatedAssets(string directory, CatalogChangeCallback? callback, ref int cataloguedAssetsBatchCount, int batchSize, List<Asset> cataloguedAssetsByPath, bool folderHasThumbnails, CancellationToken? token = null)
    {
        string[] updatedFileNames = _directoryComparer.GetUpdatedFileNames(cataloguedAssetsByPath); // TODO: Should not depend on it to have file info for each files -> break content in separate parts
        //Folder folder = _assetRepository.GetFolderByPath(directory);

        foreach (string fileName in updatedFileNames)
        {
            // TODO: Only batchSize has been tested, it has to wait the IsCancellationRequested rework to full test the condition
            if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
            {
                break;
            }

            _assetRepository.DeleteAsset(directory, fileName);
            _backupHasSameContent = false;
            string fullPath = Path.Combine(directory, fileName);

            if (_storageService.FileExists(fullPath))
            {
                Asset? updatedAsset = CreateAsset(directory, fileName);

                if (updatedAsset == null)
                {
                    continue;
                }

                // TODO: Move from here and split _directoryComparer.GetUpdatedFileNames usage above !!
                _storageService.LoadFileInformation(updatedAsset);

                updatedAsset.ImageData = LoadThumbnail(directory, fileName, updatedAsset.ThumbnailPixelWidth, updatedAsset.ThumbnailPixelHeight);

                // TODO: Check this condition about folderHasThumbnails (seems to be useless here because already added above)
                // if (!folderHasThumbnails)
                // {
                //     cataloguedAssetsByPath.Add(updatedAsset);
                // }

                cataloguedAssetsByPath = _assetRepository.GetCataloguedAssetsByPath(directory);

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
        string[] deletedFileNames = _directoryComparer.GetDeletedFileNames(fileNames, cataloguedAssetsByPath);

        foreach (string fileName in deletedFileNames)
        {
            // TODO: Only batchSize has been tested, it has to wait the IsCancellationRequested rework to full test the condition
            if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
            {
                break;
            }

            _assetRepository.DeleteAsset(directory, fileName);

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

        if (_assetRepository.ContainsThumbnail(directoryName, fileName))
        {
            thumbnailImage = _assetRepository.LoadThumbnail(directoryName, fileName, width, height);
        }

        return thumbnailImage;
    }
    #endregion
}
