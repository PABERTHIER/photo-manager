using Microsoft.Extensions.Logging;

namespace PhotoManager.Domain;

public sealed class CatalogAssetsService : ICatalogAssetsService, IDisposable
{
    private readonly IAssetRepository _assetRepository;
    private readonly IFileOperationsService _fileOperationsService;
    private readonly IImageMetadataService _imageMetadataService;
    private readonly IAssetCreationService _assetCreationService;
    private readonly IUserConfigurationService _userConfigurationService;
    private readonly IAssetsComparator _assetsComparator;
    private readonly ILogger<CatalogAssetsService> _logger;

    private bool _backupHasSameContent = true;
    private string _currentFolderPath = string.Empty;
    private readonly HashSet<string> _directories = [];
    private List<Asset> _cataloguedAssetsByPath = [];
    private readonly IDisposable _assetsUpdatedSubscription;
    private bool _suppressReactiveUpdates;

    public CatalogAssetsService(
        IAssetRepository assetRepository,
        IFileOperationsService fileOperationsService,
        IImageMetadataService imageMetadataService,
        IAssetCreationService assetCreationService,
        IUserConfigurationService userConfigurationService,
        IAssetsComparator assetsComparator,
        ILogger<CatalogAssetsService> logger)
    {
        _assetRepository = assetRepository;
        _fileOperationsService = fileOperationsService;
        _imageMetadataService = imageMetadataService;
        _assetCreationService = assetCreationService;
        _userConfigurationService = userConfigurationService;
        _assetsComparator = assetsComparator;
        _logger = logger;

        _assetsUpdatedSubscription = assetRepository.AssetsUpdated.Subscribe(_ => UpdateAssets());
    }

    public void Dispose()
    {
        _assetsUpdatedSubscription.Dispose();
    }

    private void UpdateAssets()
    {
        if (_suppressReactiveUpdates)
        {
            return;
        }

        _cataloguedAssetsByPath = _assetRepository.GetCataloguedAssetsByPath(_currentFolderPath);
    }

    public async Task CatalogAssetsAsync(CatalogChangeCallback callback, CancellationToken token = default)
    {
        await Task.Run(() =>
        {
            int cataloguedAssetsBatchCount = 0;
            HashSet<string> visitedDirectories = [];

            _suppressReactiveUpdates = true;

            try
            {
                token.ThrowIfCancellationRequested();

                HashSet<string> foldersPathToCatalog = GetFoldersPathToCatalog();

                foreach (string path in foldersPathToCatalog)
                {
                    token.ThrowIfCancellationRequested();

                    CatalogFolders(path, callback, ref cataloguedAssetsBatchCount, visitedDirectories, token);
                    callback(new()
                    {
                        Reason = CatalogChangeReason.FolderInspectionCompleted,
                        Message = $"Folder inspection for {path}, subfolders included, has been completed."
                    });
                }

                _directories.UnionWith(visitedDirectories);

                if (!_assetRepository.BackupExists() || !_backupHasSameContent)
                {
                    callback(!_assetRepository.BackupExists()
                        ? new()
                        {
                            Reason = CatalogChangeReason.BackupCreationStarted,
                            Message = "Creating catalog backup..."
                        }
                        : new CatalogChangeCallbackEventArgs
                        {
                            Reason = CatalogChangeReason.BackupUpdateStarted,
                            Message = "Updating catalog backup..."
                        });

                    _assetRepository.WriteBackup();
                    callback(new()
                    {
                        Reason = CatalogChangeReason.BackupCompleted,
                        Message = "Backup completed successfully."
                    });

                    _backupHasSameContent = true;
                }
                else
                {
                    callback(new()
                    {
                        Reason = CatalogChangeReason.NoBackupChangesDetected,
                        Message = "No changes made to the backup."
                    });
                }
            }
            catch (OperationCanceledException)
            {
                // When cancellation is requested (typically the user closing the window), the in-flight folder
                // may have produced uncommitted changes in the repository. Persist them before propagating so
                // that work already done is not lost (resilience). Backup writing is intentionally skipped:
                // it is expensive and can be performed on the next start.
                try
                {
                    Folder? currentFolder = string.IsNullOrEmpty(_currentFolderPath)
                        ? null
                        : _assetRepository.GetFolderByPath(_currentFolderPath);

                    if (_assetRepository.HasChanges())
                    {
                        _assetRepository.SaveCatalog(currentFolder);
                    }
                }
                catch (Exception exception)
                {
                    // Saving on cancellation is best-effort: log but never mask the OperationCanceledException.
                    _logger.LogError(exception, "Failed to save catalog while handling cancellation: {ExMessage}",
                        exception.Message);
                }

                callback(new()
                {
                    Reason = CatalogChangeReason.CatalogProcessCancelled,
                    Message = "The catalog process has been cancelled."
                });

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ExMessage}", ex.Message);
                callback(new()
                {
                    Reason = CatalogChangeReason.CatalogProcessFailed,
                    Message = "The catalog process has failed.",
                    Exception = ex
                });
            }
            finally
            {
                _suppressReactiveUpdates = false;

                callback(new()
                {
                    Reason = CatalogChangeReason.CatalogProcessEnded,
                    Message = "The catalog process has ended."
                });
            }
        }, token);
    }

    #region private

    private HashSet<string> GetFoldersPathToCatalog()
    {
        string[] rootPaths = _userConfigurationService.GetRootCatalogFolderPaths();

        foreach (string root in rootPaths)
        {
            if (!_assetRepository.FolderExists(root))
            {
                _assetRepository.AddFolder(root);
            }
        }

        return _assetRepository.GetFoldersPath();
    }

    private void CatalogFolders(string directory, CatalogChangeCallback callback, ref int cataloguedAssetsBatchCount,
        HashSet<string> visitedDirectories, CancellationToken token = default)
    {
        int batchSize = _userConfigurationService.AssetSettings.CatalogBatchSize;
        _currentFolderPath = directory;
        // Explicit refresh for the new folder (not suppressed by the reactive guard)
        _cataloguedAssetsByPath = _assetRepository.GetCataloguedAssetsByPath(_currentFolderPath);

        if (_fileOperationsService.FolderExists(directory))
        {
            CatalogExistingFolder(directory, callback, ref cataloguedAssetsBatchCount, batchSize, visitedDirectories,
                token);
        }
        else if (!string.IsNullOrEmpty(directory))
        {
            CatalogNonExistingFolder(directory, callback, ref cataloguedAssetsBatchCount, batchSize, token);
        }

        visitedDirectories.Add(directory);
    }

    private void CatalogExistingFolder(string directory, CatalogChangeCallback callback,
        ref int cataloguedAssetsBatchCount, int batchSize, HashSet<string> visitedDirectories,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        if (cataloguedAssetsBatchCount >= batchSize)
        {
            return;
        }

        Folder? folder = _assetRepository.GetFolderByPath(directory);

        if (folder == null)
        {
            folder = _assetRepository.AddFolder(directory);

            callback(new()
            {
                Folder = folder,
                Reason = CatalogChangeReason.FolderCreated,
                Message = $"Folder {directory} added to catalog."
            });
        }

        callback(new()
        {
            Folder = folder,
            Reason = CatalogChangeReason.FolderInspectionInProgress,
            Message = $"Inspecting folder {directory}."
        });

        string[] fileNames = _fileOperationsService.GetFileNames(directory);

        CatalogNewAssets(directory, callback, ref cataloguedAssetsBatchCount, batchSize, fileNames, token);

        _imageMetadataService.UpdateAssetsFileProperties(_cataloguedAssetsByPath);
        string[] updatedFileNames = _assetsComparator.GetUpdatedFileNames(_cataloguedAssetsByPath);

        CatalogUpdatedAssets(directory, callback, ref cataloguedAssetsBatchCount, batchSize, updatedFileNames, token);

        string[] deletedFileNames = _assetsComparator.GetDeletedFileNames(fileNames, _cataloguedAssetsByPath);

        CatalogDeletedAssets(directory, callback, ref cataloguedAssetsBatchCount, batchSize, deletedFileNames, token);

        // folder is guaranteed non-null (assigned via AddFolder or GetFolderByPath on a known-existing path)
        bool isBlobFileExists = _assetRepository.IsBlobFileExists(folder.BlobFileName);

        if (_assetRepository.HasChanges() || !isBlobFileExists)
        {
            _assetRepository.SaveCatalog(folder);
        }

        token.ThrowIfCancellationRequested();

        if (cataloguedAssetsBatchCount >= batchSize)
        {
            return;
        }

        IEnumerable<DirectoryInfo> subdirectories = new DirectoryInfo(directory).EnumerateDirectories();

        foreach (DirectoryInfo subdirectory in subdirectories)
        {
            token.ThrowIfCancellationRequested();

            if (!_directories.Contains(subdirectory.FullName))
            {
                CatalogFolders(subdirectory.FullName, callback, ref cataloguedAssetsBatchCount, visitedDirectories,
                    token);
            }
        }
    }

    private void CatalogNonExistingFolder(string directory, CatalogChangeCallback callback,
        ref int cataloguedAssetsBatchCount, int batchSize, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        if (cataloguedAssetsBatchCount >= batchSize)
        {
            return;
        }

        // If the directory has been manually deleted during the EnumerateDirectories, we want to throw an exception
        Folder folder = _assetRepository.GetFolderByPath(directory)!;

        List<Asset> assetsToProcess = [.. _cataloguedAssetsByPath];

        foreach (Asset asset in assetsToProcess)
        {
            token.ThrowIfCancellationRequested();

            if (cataloguedAssetsBatchCount >= batchSize)
            {
                break;
            }

            _ = _assetRepository.DeleteAsset(directory, asset.FileName);
            _cataloguedAssetsByPath.Remove(asset);
            _backupHasSameContent = false;
            cataloguedAssetsBatchCount++;

            callback(new()
            {
                Asset = asset,
                CataloguedAssetsByPath = [.. _cataloguedAssetsByPath],
                Reason = CatalogChangeReason.AssetDeleted,
                Message = $"Image {Path.Combine(directory, asset.FileName)} deleted from catalog."
            });
        }

        if (_cataloguedAssetsByPath.Count == 0)
        {
            _assetRepository.DeleteFolder(folder);
            _directories.Remove(folder.Path);

            callback(new()
            {
                Folder = folder,
                Reason = CatalogChangeReason.FolderDeleted,
                Message = $"Folder {directory} deleted from catalog."
            });
        }

        if (_assetRepository.HasChanges())
        {
            _assetRepository.SaveCatalog(folder);
        }
    }

    private void CatalogNewAssets(string directory, CatalogChangeCallback callback, ref int cataloguedAssetsBatchCount,
        int batchSize, string[] fileNames, CancellationToken token = default)
    {
        (string[] imageNames, string[] videoNames) = _assetsComparator.GetImageAndVideoNames(fileNames);

        string[] newImageFileNames = _assetsComparator.GetNewFileNames(imageNames, _cataloguedAssetsByPath);
        string[] newVideoFileNames = _assetsComparator.GetNewFileNames(videoNames, _cataloguedAssetsByPath);

        CatalogAssets(newImageFileNames, false, directory, callback, ref cataloguedAssetsBatchCount, batchSize, token);

        if (_userConfigurationService.AssetSettings.AnalyseVideos)
        {
            CatalogAssets(newVideoFileNames, true, directory, callback, ref cataloguedAssetsBatchCount, batchSize,
                token);
        }
    }

    private void CatalogAssets(IEnumerable<string> fileNames, bool isAssetVideo, string directory,
        CatalogChangeCallback callback, ref int cataloguedAssetsBatchCount, int batchSize,
        CancellationToken token = default)
    {
        foreach (string fileName in fileNames)
        {
            token.ThrowIfCancellationRequested();

            if (cataloguedAssetsBatchCount >= batchSize)
            {
                break;
            }

            string fullPath = Path.Combine(directory, fileName);
            Asset? newAsset = _assetCreationService.CreateAsset(directory, fileName, isAssetVideo,
                skipCatalogCheck: true);

            if (newAsset == null)
            {
                if (!isAssetVideo)
                {
                    callback(new()
                    {
                        CataloguedAssetsByPath = [.. _cataloguedAssetsByPath],
                        Reason = CatalogChangeReason.AssetNotCreated,
                        Message = $"Image {fullPath} not added to catalog (corrupted)."
                    });
                }

                continue;
            }

            _cataloguedAssetsByPath.Add(newAsset);

            callback(new()
            {
                Asset = newAsset,
                CataloguedAssetsByPath = [.. _cataloguedAssetsByPath],
                Reason = CatalogChangeReason.AssetCreated,
                Message = $"Image {fullPath} added to catalog."
            });

            _backupHasSameContent = false;
            cataloguedAssetsBatchCount++;
        }
    }

    private void CatalogUpdatedAssets(string directory, CatalogChangeCallback callback,
        ref int cataloguedAssetsBatchCount, int batchSize, IEnumerable<string> updatedFileNames,
        CancellationToken token = default)
    {
        foreach (string fileName in updatedFileNames)
        {
            token.ThrowIfCancellationRequested();

            if (cataloguedAssetsBatchCount >= batchSize)
            {
                break;
            }

            string fullPath = Path.Combine(directory, fileName);

            Asset? deletedAsset = _assetRepository.DeleteAsset(directory, fileName);
            _cataloguedAssetsByPath.RemoveAll(a => a.FileName == fileName);
            _backupHasSameContent = false;

            Asset? updatedAsset = _assetCreationService.CreateAsset(directory, fileName,
                skipCatalogCheck: true);

            if (updatedAsset == null)
            {
                callback(new()
                {
                    Asset = deletedAsset,
                    CataloguedAssetsByPath = [.. _cataloguedAssetsByPath],
                    Reason = CatalogChangeReason.AssetDeleted,
                    Message = $"Image {fullPath} deleted from catalog (corrupted)."
                });

                continue;
            }

            _cataloguedAssetsByPath.Add(updatedAsset);

            callback(new()
            {
                Asset = updatedAsset,
                CataloguedAssetsByPath = [.. _cataloguedAssetsByPath],
                Reason = CatalogChangeReason.AssetUpdated,
                Message = $"Image {fullPath} updated in catalog."
            });

            cataloguedAssetsBatchCount++;
        }
    }

    private void CatalogDeletedAssets(string directory, CatalogChangeCallback callback,
        ref int cataloguedAssetsBatchCount, int batchSize, IEnumerable<string> deletedFileNames,
        CancellationToken token = default)
    {
        foreach (string fileName in deletedFileNames)
        {
            token.ThrowIfCancellationRequested();

            if (cataloguedAssetsBatchCount >= batchSize)
            {
                break;
            }

            string fullPath = Path.Combine(directory, fileName);

            Asset? deletedAsset = _assetRepository.DeleteAsset(directory, fileName);
            _cataloguedAssetsByPath.RemoveAll(a => a.FileName == fileName);

            callback(new()
            {
                Asset = deletedAsset,
                CataloguedAssetsByPath = [.. _cataloguedAssetsByPath],
                Reason = CatalogChangeReason.AssetDeleted,
                Message = $"Image {fullPath} deleted from catalog."
            });

            _backupHasSameContent = false;
            cataloguedAssetsBatchCount++;
        }
    }

    #endregion
}
