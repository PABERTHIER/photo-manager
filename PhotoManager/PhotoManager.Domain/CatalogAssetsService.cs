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
    }

    public async Task CatalogAssetsAsync(CatalogChangeCallback? callback, CancellationToken? token = null)
    {
        // TODO: Improve Message for each event
        await Task.Run(() =>
        {
            int cataloguedAssetsBatchCount = 0;
            List<string> visitedFolders = new();

            try
            {
                Folder[] foldersToCatalog = GetFoldersToCatalog();
                // TODO: Since the root folders to catalog are combined in the same list
                // with the catalogued sub-folders, the catalog process should keep a list
                // of the already visited folders so they don't get catalogued twice
                // in the same execution.

                foreach (Folder folder in foldersToCatalog)
                {
                    // ThrowIfCancellationRequested should be in each if below ?
                    // token?.ThrowIfCancellationRequested(); rework all the cancellation
                    cataloguedAssetsBatchCount = CatalogAssets(folder.Path, callback, cataloguedAssetsBatchCount, visitedFolders, token);
                }

                if (!_assetRepository.BackupExists() || !_backupHasSameContent)
                {
                    callback?.Invoke(!_assetRepository.BackupExists()
                        ? new CatalogChangeCallbackEventArgs { Message = "Creating catalog backup..." }
                        : new CatalogChangeCallbackEventArgs { Message = "Updating catalog backup..." });

                    _assetRepository.WriteBackup();
                    callback?.Invoke(new CatalogChangeCallbackEventArgs { Message = string.Empty });

                    _backupHasSameContent = true;
                }

                callback?.Invoke(new CatalogChangeCallbackEventArgs { Message = string.Empty });
            }
            catch (OperationCanceledException)
            {
                // If the catalog background process is cancelled,
                // there is a risk that it happens while saving the catalog files.
                // This could result in the files being damaged.
                // Therefore the application saves the files before the task is completely shut down.
                Folder? currentFolder = _assetRepository.GetFolderByPath(_currentFolderPath);
                _assetRepository.SaveCatalog(currentFolder);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                callback?.Invoke(new CatalogChangeCallbackEventArgs { Exception = ex });
            }
            finally
            {
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

    private int CatalogAssets(string directory, CatalogChangeCallback? callback, int cataloguedAssetsBatchCount, List<string> visitedFolders, CancellationToken? token = null)
    {
        if (visitedFolders.Contains(directory))
        {
            return cataloguedAssetsBatchCount;
        }

        _currentFolderPath = directory;
        int batchSize = _userConfigurationService.AssetSettings.CatalogBatchSize;

        if (_storageService.FolderExists(directory))
        {
            cataloguedAssetsBatchCount = CatalogExistingFolder(directory, callback, cataloguedAssetsBatchCount, batchSize, visitedFolders, token);
        }
        else if (!string.IsNullOrEmpty(directory) && !_storageService.FolderExists(directory))
        {
            cataloguedAssetsBatchCount = CatalogNonExistingFolder(directory, callback, cataloguedAssetsBatchCount, batchSize, token);
        }

        visitedFolders.Add(directory);

        return cataloguedAssetsBatchCount;
    }

    private int CatalogExistingFolder(string directory, CatalogChangeCallback? callback, int cataloguedAssetsBatchCount, int batchSize, List<string> visitedFolders, CancellationToken? token = null)
    {
        Folder? folder;

        if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
        {
            return cataloguedAssetsBatchCount;
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

        callback?.Invoke(new CatalogChangeCallbackEventArgs
        {
            Folder = folder,
            Message = $"Inspecting folder {directory}."
        });

        List<Asset> cataloguedAssets = _assetRepository.GetCataloguedAssetsByPath(directory);

        bool folderHasThumbnails = folder != null && _assetRepository.FolderHasThumbnails(folder);

        if (!folderHasThumbnails)
        {
            foreach (Asset asset in cataloguedAssets)
            {
                asset.ImageData = LoadThumbnail(directory, asset.FileName, asset.ThumbnailPixelWidth, asset.ThumbnailPixelHeight);
            }
        }

        string[] filesName = _storageService.GetFileNames(directory);

        cataloguedAssetsBatchCount = CatalogNewAssets(directory, callback, cataloguedAssetsBatchCount, batchSize, filesName, cataloguedAssets, folderHasThumbnails, token);
        cataloguedAssetsBatchCount = CatalogUpdatedAssets(directory, callback, cataloguedAssetsBatchCount, batchSize, cataloguedAssets, folderHasThumbnails, token);
        cataloguedAssetsBatchCount = CatalogDeletedAssets(directory, callback, cataloguedAssetsBatchCount, batchSize, filesName, folder, cataloguedAssets, token);

        if (_assetRepository.HasChanges() || !folderHasThumbnails)
        {
            _assetRepository.SaveCatalog(folder);
        }

        if (cataloguedAssetsBatchCount >= batchSize || (!(!token?.IsCancellationRequested ?? true)))
        {
            return cataloguedAssetsBatchCount;
        }

        IEnumerable<DirectoryInfo> subdirectories = new DirectoryInfo(directory).EnumerateDirectories();

        foreach (DirectoryInfo subdirectory in subdirectories)
        {
            cataloguedAssetsBatchCount = CatalogAssets(subdirectory.FullName, callback, cataloguedAssetsBatchCount, visitedFolders, token);
        }

        return cataloguedAssetsBatchCount;
    }

    private int CatalogNonExistingFolder(string directory, CatalogChangeCallback? callback, int cataloguedAssetsBatchCount, int batchSize, CancellationToken? token = null)
    {
        if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
        {
            return cataloguedAssetsBatchCount;
        }

        // If the folder doesn't exist anymore, the corresponding entry in the catalog and the thumbnails file are both deleted.
        // TODO: This should be tested in a new test method, in which the non existent folder is explicitly added to the catalog.
        Folder? folder = _assetRepository.GetFolderByPath(directory);

        if (folder != null)
        {
            List<Asset> cataloguedAssets = _assetRepository.GetCataloguedAssetsByPath(directory);

            foreach (var asset in cataloguedAssets)
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
                    CataloguedAssets = cataloguedAssets,
                    Message = $"Image {Path.Combine(directory, asset.FileName)} deleted from catalog.",
                    Reason = ReasonEnum.AssetDeleted
                });
            }

            cataloguedAssets = _assetRepository.GetCataloguedAssetsByPath(directory);

            if (cataloguedAssets.Count == 0)
            {
                _assetRepository.DeleteFolder(folder);

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

        return cataloguedAssetsBatchCount;
    }

    private int CatalogNewAssets(string directory, CatalogChangeCallback? callback, int cataloguedAssetsBatchCount, int batchSize, string[] fileNames, List<Asset> cataloguedAssets, bool folderHasThumbnails, CancellationToken? token = null)
    {
        string[] imageNames;
        string[] videoNames;

        (imageNames, videoNames) = _directoryComparer.GetImageAndVideoNames(fileNames);

        string[] newImageFileNames = _directoryComparer.GetNewFileNames(imageNames, cataloguedAssets);
        string[] newVideoFileNames = _directoryComparer.GetNewFileNames(videoNames, cataloguedAssets);

        cataloguedAssetsBatchCount += CreateAssets(newImageFileNames, false, directory, callback, cataloguedAssetsBatchCount, batchSize, cataloguedAssets, folderHasThumbnails, token);

        if (_userConfigurationService.AssetSettings.AnalyseVideos)
        {
            cataloguedAssetsBatchCount += CreateAssets(newVideoFileNames, true, directory, callback, cataloguedAssetsBatchCount, batchSize, cataloguedAssets, folderHasThumbnails, token);
        }

        return cataloguedAssetsBatchCount;
    }

    private int CreateAssets(string[] fileNames, bool isAssetVideo, string directory, CatalogChangeCallback? callback, int cataloguedAssetsBatchCount, int batchSize, List<Asset> cataloguedAssets, bool folderHasThumbnails, CancellationToken? token = null)
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

            if (!folderHasThumbnails)
            {
                cataloguedAssets.Add(newAsset);
            }

            callback?.Invoke(new CatalogChangeCallbackEventArgs
            {
                Asset = newAsset,
                CataloguedAssets = cataloguedAssets,
                Message = $"Image {Path.Combine(directory, newAsset.FileName)} added to catalog.",
                Reason = ReasonEnum.AssetCreated
            });

            _backupHasSameContent = false;
            cataloguedAssetsBatchCount++;
        }

        return cataloguedAssetsBatchCount;
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

    private int CatalogUpdatedAssets(string directory, CatalogChangeCallback? callback, int cataloguedAssetsBatchCount, int batchSize, List<Asset> cataloguedAssets, bool folderHasThumbnails, CancellationToken? token = null)
    {
        string[] updatedFileNames = _directoryComparer.GetUpdatedFileNames(cataloguedAssets); // TODO: Should not depend on it to have file info for each files -> break content in separate parts
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

                updatedAsset.ImageData = LoadThumbnail(directory, fileName, updatedAsset.ThumbnailPixelWidth, updatedAsset.ThumbnailPixelHeight);

                if (!folderHasThumbnails)
                {
                    cataloguedAssets.Add(updatedAsset);
                }

                callback?.Invoke(new CatalogChangeCallbackEventArgs
                {
                    Asset = updatedAsset,
                    CataloguedAssets = cataloguedAssets,
                    Message = $"Image {fullPath} updated in catalog.",
                    Reason = ReasonEnum.AssetUpdated
                });

                cataloguedAssetsBatchCount++;
            }
        }

        return cataloguedAssetsBatchCount;
    }

    private int CatalogDeletedAssets(string directory, CatalogChangeCallback? callback, int cataloguedAssetsBatchCount, int batchSize, string[] fileNames, Folder folder, List<Asset> cataloguedAssets, CancellationToken? token = null)
    {
        string[] deletedFileNames = _directoryComparer.GetDeletedFileNames(fileNames, cataloguedAssets);

        foreach (string fileName in deletedFileNames)
        {
            // TODO: Only batchSize has been tested, it has to wait the IsCancellationRequested rework to full test the condition
            if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
            {
                break;
            }

            Asset deletedAsset = new()
            {
                FileName = fileName,
                FolderId = folder.FolderId,
                Folder = folder
            };

            _assetRepository.DeleteAsset(directory, fileName);

            callback?.Invoke(new CatalogChangeCallbackEventArgs
            {
                Asset = deletedAsset,
                Message = $"Image {Path.Combine(directory, fileName)} deleted from catalog.",
                Reason = ReasonEnum.AssetDeleted
            });

            _backupHasSameContent = false;
            cataloguedAssetsBatchCount++;
        }

        return cataloguedAssetsBatchCount;
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
