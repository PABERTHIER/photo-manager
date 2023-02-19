using log4net;
using PhotoManager.Constants;
using PhotoManager.Domain.Interfaces;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace PhotoManager.Domain;

public class CatalogAssetsService : ICatalogAssetsService
{
    private readonly IAssetRepository _assetRepository;
    private readonly IAssetHashCalculatorService _assetHashCalculatorService;
    private readonly IStorageService _storageService;
    private readonly IUserConfigurationService _userConfigurationService;
    private readonly IDirectoryComparer _directoryComparer;

    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private string currentFolderPath;

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
    }

    public async Task CatalogAssetsAsync(CatalogChangeCallback callback, CancellationToken? token = null)
    {
        await Task.Run(() =>
        {
            int cataloguedAssetsBatchCount = 0;
            List<string> visitedFolders = new();

            try
            {
                if (!_assetRepository.BackupExists())
                {
                    callback?.Invoke(new CatalogChangeCallbackEventArgs() { Message = "Creating catalog backup..." });
                    _assetRepository.WriteBackup();
                    callback?.Invoke(new CatalogChangeCallbackEventArgs() { Message = string.Empty });
                }

                Folder[] foldersToCatalog = GetFoldersToCatalog();

                // TODO: Since the root folders to catalog are combined in the same list
                // with the catalogued sub-folders, the catalog process should keep a list
                // of the already visited folders so they don't get catalogued twice
                // in the same execution.

                foreach (Folder folder in foldersToCatalog)
                {
                    cataloguedAssetsBatchCount = CatalogAssets(folder.Path, callback, cataloguedAssetsBatchCount, visitedFolders, token);
                }

                callback?.Invoke(new CatalogChangeCallbackEventArgs() { Message = string.Empty });
            }
            catch (OperationCanceledException)
            {
                // If the catalog background process is cancelled,
                // there is a risk that it happens while saving the catalog files.
                // This could result in the files being damaged.
                // Therefore the application saves the files before the task is completly shut down.
                Folder currentFolder = _assetRepository.GetFolderByPath(currentFolderPath);
                _assetRepository.SaveCatalog(currentFolder);
                throw;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                callback?.Invoke(new CatalogChangeCallbackEventArgs { Exception = ex });
            }
            finally
            {
                callback?.Invoke(new CatalogChangeCallbackEventArgs { Message = string.Empty });
            }
        });
    }

    public Asset? CreateAsset(string directoryName, string fileName)
    {
        Asset asset = null;

        if (!_assetRepository.IsAssetCatalogued(directoryName, fileName))
        {
            string imagePath = Path.Combine(directoryName, fileName);
            byte[] imageBytes = _storageService.GetFileBytes(imagePath);

            if (!_storageService.GetIsValidGDIPlusImage(imageBytes))
            {
                return asset;
            }

            ushort exifOrientation = _storageService.GetExifOrientation(imageBytes);
            Rotation rotation = _storageService.GetImageRotation(exifOrientation);

            bool assetCorrupted = false;
            bool assetRotated =  false;

            if (exifOrientation == AssetConstants.OrientationCorruptedImage)
            {
                assetCorrupted = true;
            }

            if (rotation != Rotation.Rotate0)
            {
                assetRotated = true;
            }

            BitmapImage originalImage = _storageService.LoadBitmapImage(imageBytes, Rotation.Rotate0); // before was rotation

            double originalDecodeWidth = originalImage.PixelWidth;
            double originalDecodeHeight = originalImage.PixelHeight;
            double thumbnailDecodeWidth;
            double thumbnailDecodeHeight;
            double percentage;

            // If the original image is landscape
            if (originalDecodeWidth > originalDecodeHeight)
            {
                thumbnailDecodeWidth = AssetConstants.MaxWidth;
                percentage = (AssetConstants.MaxWidth * 100d / originalDecodeWidth);
                thumbnailDecodeHeight = (percentage * originalDecodeHeight) / 100d;
            }
            else // If the original image is portrait
            {
                thumbnailDecodeHeight = AssetConstants.MaxHeight;
                percentage = (AssetConstants.MaxHeight * 100d / originalDecodeHeight);
                thumbnailDecodeWidth = (percentage * originalDecodeWidth) / 100d;
            }

            BitmapImage thumbnailImage = _storageService.LoadBitmapThumbnailImage(imageBytes,
                Rotation.Rotate0, // before was rotation
                Convert.ToInt32(thumbnailDecodeWidth),
                Convert.ToInt32(thumbnailDecodeHeight));
            bool isPng = imagePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
            byte[] thumbnailBuffer = isPng ? _storageService.GetPngBitmapImage(thumbnailImage) : _storageService.GetJpegBitmapImage(thumbnailImage);
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
                AssetCorruptedMessage = assetCorrupted ? AssetConstants.AssetCorruptedMessage : null,
                IsAssetRotated = assetRotated,
                AssetRotatedMessage = assetRotated ? AssetConstants.AssetRotatedMessage : null,
            };

            _assetRepository.AddAsset(asset, thumbnailBuffer);
        }

        return asset;
    }

    // TODO: Do it for video by keeping first frame and save it as asset ou video object ?
    public VideoAsset? CreateVideoAsset(string directoryName, string fileName)
    {
        VideoAsset videoAsset = null;

        if (!_assetRepository.IsAssetCatalogued(directoryName, fileName))
        {
            string videoPath = Path.Combine(directoryName, fileName);
            byte[] imageBytes = _storageService.GetFileBytes(videoPath);

            if (!_storageService.GetIsValidGDIPlusImage(imageBytes))
            {
                return videoAsset;
            }

            ushort exifOrientation = _storageService.GetExifOrientation(imageBytes);
            Rotation rotation = _storageService.GetImageRotation(exifOrientation);
            BitmapImage originalImage = _storageService.LoadBitmapImage(imageBytes, rotation);

            double originalDecodeWidth = originalImage.PixelWidth;
            double originalDecodeHeight = originalImage.PixelHeight;
            double thumbnailDecodeWidth;
            double thumbnailDecodeHeight;
            double percentage;

            // If the original image is landscape
            if (originalDecodeWidth > originalDecodeHeight)
            {
                thumbnailDecodeWidth = AssetConstants.MaxWidth;
                percentage = (AssetConstants.MaxWidth * 100d / originalDecodeWidth);
                thumbnailDecodeHeight = (percentage * originalDecodeHeight) / 100d;
            }
            else // If the original image is portrait
            {
                thumbnailDecodeHeight = AssetConstants.MaxHeight;
                percentage = (AssetConstants.MaxHeight * 100d / originalDecodeHeight);
                thumbnailDecodeWidth = (percentage * originalDecodeWidth) / 100d;
            }

            BitmapImage thumbnailImage = _storageService.LoadBitmapThumbnailImage(imageBytes,
                rotation,
                Convert.ToInt32(thumbnailDecodeWidth),
                Convert.ToInt32(thumbnailDecodeHeight));
            bool isPng = videoPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
            byte[] thumbnailBuffer = isPng ? _storageService.GetPngBitmapImage(thumbnailImage) : _storageService.GetJpegBitmapImage(thumbnailImage);
            Folder folder = _assetRepository.GetFolderByPath(directoryName);

            videoAsset = new VideoAsset
            {
                FileName = Path.GetFileName(videoPath),
                FolderId = folder.FolderId,
                Folder = folder,
                FileSize = new FileInfo(videoPath).Length,
                PixelWidth = Convert.ToInt32(originalDecodeWidth),
                PixelHeight = Convert.ToInt32(originalDecodeHeight),
                ThumbnailPixelWidth = Convert.ToInt32(thumbnailDecodeWidth),
                ThumbnailPixelHeight = Convert.ToInt32(thumbnailDecodeHeight),
                ImageRotation = rotation,
                ThumbnailCreationDateTime = DateTime.Now,
                Hash = _assetHashCalculatorService.CalculateVideoHash(videoPath)
            };

            _assetRepository.AddVideoAsset(videoAsset, thumbnailBuffer);
        }

        return videoAsset;
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

    private int CatalogAssets(string directory, CatalogChangeCallback callback, int cataloguedAssetsBatchCount, List<string> visitedFolders, CancellationToken? token = null)
    {
        if (!visitedFolders.Contains(directory))
        {
            currentFolderPath = directory;
            int batchSize = _userConfigurationService.GetCatalogBatchSize();

            if (_storageService.FolderExists(directory))
            {
                cataloguedAssetsBatchCount = CatalogExistingFolder(directory, callback, cataloguedAssetsBatchCount, batchSize, visitedFolders, token);
            }
            else if (!string.IsNullOrEmpty(directory) && !_storageService.FolderExists(directory))
            {
                cataloguedAssetsBatchCount = CatalogNonExistingFolder(directory, callback, cataloguedAssetsBatchCount, batchSize, token);
            }

            visitedFolders.Add(directory);
        }

        return cataloguedAssetsBatchCount;
    }

    private int CatalogExistingFolder(string directory, CatalogChangeCallback callback, int cataloguedAssetsBatchCount, int batchSize, List<string> visitedFolders, CancellationToken? token = null)
    {
        Folder folder;

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
                Message = $"Folder {directory} added to catalog",
                Reason = ReasonEnum.FolderCreated
            });
        }

        callback?.Invoke(new CatalogChangeCallbackEventArgs() { Message = "Inspecting folder " + directory });
        string[] fileNames = _storageService.GetFileNames(directory);
        folder = _assetRepository.GetFolderByPath(directory);
        List<Asset> cataloguedAssets = _assetRepository.GetCataloguedAssets(directory);
        bool folderHasThumbnails = _assetRepository.FolderHasThumbnails(folder);

        if (!folderHasThumbnails)
        {
            foreach (var asset in cataloguedAssets)
            {
                asset.ImageData = LoadThumbnail(directory, asset.FileName, asset.ThumbnailPixelWidth, asset.ThumbnailPixelHeight);
            }
        }

        cataloguedAssetsBatchCount = CatalogNewAssets(directory, callback, cataloguedAssetsBatchCount, batchSize, fileNames, cataloguedAssets, folderHasThumbnails, token);
        cataloguedAssetsBatchCount = CatalogUpdatedAssets(directory, callback, cataloguedAssetsBatchCount, batchSize, fileNames, cataloguedAssets, folderHasThumbnails, token);
        cataloguedAssetsBatchCount = CatalogDeletedAssets(directory, callback, cataloguedAssetsBatchCount, batchSize, fileNames, folder, cataloguedAssets, token);

        if (_assetRepository.HasChanges() || !folderHasThumbnails)
        {
            _assetRepository.SaveCatalog(folder);
        }

        if (cataloguedAssetsBatchCount < batchSize && (!token?.IsCancellationRequested ?? true))
        {
            var subdirectories = new DirectoryInfo(directory).EnumerateDirectories();

            foreach (var subdir in subdirectories)
            {
                cataloguedAssetsBatchCount = CatalogAssets(subdir.FullName, callback, cataloguedAssetsBatchCount, visitedFolders, token);
            }
        }

        return cataloguedAssetsBatchCount;
    }

    private int CatalogNonExistingFolder(string directory, CatalogChangeCallback callback, int cataloguedAssetsBatchCount, int batchSize, CancellationToken? token = null)
    {
        if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
        {
            return cataloguedAssetsBatchCount;
        }

        // If the folder doesn't exist anymore, the corresponding entry in the catalog and the thumbnails file are both deleted.
        // TODO: This should be tested in a new test method, in which the non existent folder is explicitly added to the catalog.
        Folder folder = _assetRepository.GetFolderByPath(directory);

        if (folder != null)
        {
            List<Asset> cataloguedAssets = _assetRepository.GetCataloguedAssets(directory);

            foreach (var asset in cataloguedAssets)
            {
                if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
                {
                    break;
                }

                _assetRepository.DeleteAsset(directory, asset.FileName);
                cataloguedAssetsBatchCount++;

                callback?.Invoke(new CatalogChangeCallbackEventArgs
                {
                    Asset = asset,
                    Message = $"Image {Path.Combine(directory, asset.FileName)} deleted from catalog",
                    Reason = ReasonEnum.AssetDeleted
                });
            }

            cataloguedAssets = _assetRepository.GetCataloguedAssets(directory);

            if (cataloguedAssets.Count == 0)
            {
                _assetRepository.DeleteFolder(folder);

                callback?.Invoke(new CatalogChangeCallbackEventArgs
                {
                    Folder = folder,
                    Message = "Folder " + directory + " deleted from catalog",
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

    private int CatalogNewAssets(string directory, CatalogChangeCallback callback, int cataloguedAssetsBatchCount, int batchSize, string[] fileNames, List<Asset> cataloguedAssets, bool folderHasThumbnails, CancellationToken? token = null)
    {
        string[] newFileNames = _directoryComparer.GetNewFileNames(fileNames, cataloguedAssets);

        foreach (var fileName in newFileNames)
        {

            if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
            {
                break;
            }

            Asset newAsset = CreateAsset(directory, fileName);
            
            if (newAsset == null)
            {
                continue;
            }

            newAsset.ImageData = LoadThumbnail(directory, fileName, newAsset.ThumbnailPixelWidth, newAsset.ThumbnailPixelHeight);

            if (!folderHasThumbnails)
            {
                cataloguedAssets.Add(newAsset);
            }

            callback?.Invoke(new CatalogChangeCallbackEventArgs
            {
                Asset = newAsset,
                CataloguedAssets = cataloguedAssets,
                Message = $"Image {Path.Combine(directory, fileName)} added to catalog",
                Reason = ReasonEnum.AssetCreated
            });

            cataloguedAssetsBatchCount++;
        }

        return cataloguedAssetsBatchCount;
    }

    private int CatalogUpdatedAssets(string directory, CatalogChangeCallback callback, int cataloguedAssetsBatchCount, int batchSize, string[] fileNames, List<Asset> cataloguedAssets, bool folderHasThumbnails, CancellationToken? token = null)
    {
        string[] updatedFileNames = _directoryComparer.GetUpdatedFileNames(fileNames, cataloguedAssets);
        Folder folder = _assetRepository.GetFolderByPath(directory);

        foreach (var fileName in updatedFileNames)
        {
            if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
            {
                break;
            }

            _assetRepository.DeleteAsset(directory, fileName);
            string fullPath = Path.Combine(directory, fileName);

            if (_storageService.FileExists(fullPath))
            {
                Asset updatedAsset = CreateAsset(directory, fileName);
                updatedAsset.ImageData = LoadThumbnail(directory, fileName, updatedAsset.ThumbnailPixelWidth, updatedAsset.ThumbnailPixelHeight);

                if (!folderHasThumbnails)
                {
                    cataloguedAssets.Add(updatedAsset);
                }

                callback?.Invoke(new CatalogChangeCallbackEventArgs
                {
                    Asset = updatedAsset,
                    CataloguedAssets = cataloguedAssets,
                    Message = $"Image {fullPath} updated in catalog",
                    Reason = ReasonEnum.AssetUpdated
                });

                cataloguedAssetsBatchCount++;
            }
        }

        return cataloguedAssetsBatchCount;
    }

    private int CatalogDeletedAssets(string directory, CatalogChangeCallback callback, int cataloguedAssetsBatchCount, int batchSize, string[] fileNames, Folder folder, List<Asset> cataloguedAssets, CancellationToken? token = null)
    {
        string[] deletedFileNames = _directoryComparer.GetDeletedFileNames(fileNames, cataloguedAssets);

        foreach (var fileName in deletedFileNames)
        {
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
                Message = $"Image {Path.Combine(directory, fileName)} deleted from catalog",
                Reason = ReasonEnum.AssetDeleted
            });

            cataloguedAssetsBatchCount++;
        }

        return cataloguedAssetsBatchCount;
    }

    private BitmapImage LoadThumbnail(string directoryName, string fileName, int width, int height)
    {
        BitmapImage thumbnailImage = null;

        if (_assetRepository.ContainsThumbnail(directoryName, fileName))
        {
            thumbnailImage = _assetRepository.LoadThumbnail(directoryName, fileName, width, height);
        }

        return thumbnailImage;
    }
    #endregion
}
