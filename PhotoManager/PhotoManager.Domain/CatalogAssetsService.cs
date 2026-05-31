using Microsoft.Extensions.Logging;
using PhotoManager.Domain.Pipeline;

namespace PhotoManager.Domain;

public sealed class CatalogAssetsService : ICatalogAssetsService, IDisposable
{
    private readonly IAssetRepository _assetRepository;
    private readonly IFileOperationsService _fileOperationsService;
    private readonly IImageMetadataService _imageMetadataService;
    private readonly IAssetCreationService _assetCreationService;
    private readonly IUserConfigurationService _userConfigurationService;
    private readonly IAssetsComparator _assetsComparator;
    private readonly CatalogFolderPipeline _catalogFolderPipeline;
    private readonly ILogger<CatalogAssetsService> _logger;

    private bool _backupHasSameContent = true;
    private string _currentFolderPath = string.Empty;
    private readonly HashSet<string> _directories = [];
    private List<Asset> _cataloguedAssetsByPath = [];
    private IReadOnlyList<Asset> _cataloguedAssetsByPathView = [];
    private readonly IDisposable _assetsUpdatedSubscription;
    private bool _suppressReactiveUpdates;

    public CatalogAssetsService(
        IAssetRepository assetRepository,
        IFileOperationsService fileOperationsService,
        IImageMetadataService imageMetadataService,
        IAssetCreationService assetCreationService,
        IUserConfigurationService userConfigurationService,
        IAssetsComparator assetsComparator,
        CatalogFolderPipeline catalogFolderPipeline,
        ILogger<CatalogAssetsService> logger)
    {
        _assetRepository = assetRepository;
        _fileOperationsService = fileOperationsService;
        _imageMetadataService = imageMetadataService;
        _assetCreationService = assetCreationService;
        _userConfigurationService = userConfigurationService;
        _assetsComparator = assetsComparator;
        _catalogFolderPipeline = catalogFolderPipeline;
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

        SetCataloguedAssetsByPath(_assetRepository.GetCataloguedAssetsByPath(_currentFolderPath));
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

                foreach (string path in foldersPathToCatalog.OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
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

        // Fast path: AssetsDirectory has not changed since the last catalog run
        string currentRootKey = string.Join("|", rootPaths);
        string? storedRootKey = _assetRepository.GetStoredAssetsDirectory();

        if (string.Equals(storedRootKey, currentRootKey, StringComparison.OrdinalIgnoreCase))
        {
            return _assetRepository.GetFoldersPath();
        }

        // Path changed (or first run): remove stale folders and store the new root
        HashSet<string> allFolderPaths = _assetRepository.GetFoldersPath();
        HashSet<string> foldersToProcess = new(StringComparer.Ordinal);
        bool hasStaleToClean = false;

        foreach (string path in allFolderPaths)
        {
            if (IsUnderAnyRoot(path, rootPaths))
            {
                foldersToProcess.Add(path);
            }
            else
            {
                hasStaleToClean = true;
                DeleteStaleFolderFromCatalog(path);
            }
        }

        if (hasStaleToClean)
        {
            _assetRepository.Vacuum();
        }

        _assetRepository.StoreAssetsDirectory(currentRootKey);

        return foldersToProcess;
    }

    private void DeleteStaleFolderFromCatalog(string path)
    {
        Folder? folder = _assetRepository.GetFolderByPath(path);

        if (folder == null)
        {
            return;
        }

        _assetRepository.DeleteFolder(folder);
        RemoveVisitedDirectory(path);
        _backupHasSameContent = false;
    }

    private void RemoveVisitedDirectory(string path)
    {
        _directories.RemoveWhere(directory => IsSamePathOrDescendant(directory, path));
    }

    private static bool IsUnderAnyRoot(string path, string[] rootPaths)
    {
        foreach (string root in rootPaths)
        {
            if (IsSamePathOrDescendant(path, root))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSamePathOrDescendant(string path, string root)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(root))
        {
            return string.Equals(path, root, StringComparison.OrdinalIgnoreCase);
        }

        string relativePath = Path.GetRelativePath(root, path);

        return string.Equals(relativePath, ".", StringComparison.Ordinal) || IsDescendantRelativePath(relativePath);
    }

    private static bool IsDescendantRelativePath(string relativePath)
    {
        string parentDirectoryPrefix = $"..{Path.DirectorySeparatorChar}";
        string parentAltDirectoryPrefix = $"..{Path.AltDirectorySeparatorChar}";

        return !string.Equals(relativePath, "..", StringComparison.Ordinal)
            && !relativePath.StartsWith(parentDirectoryPrefix, StringComparison.Ordinal)
            && !relativePath.StartsWith(parentAltDirectoryPrefix, StringComparison.Ordinal)
            && !Path.IsPathRooted(relativePath);
    }

    private void CatalogFolders(string directory, CatalogChangeCallback callback, ref int cataloguedAssetsBatchCount,
        HashSet<string> visitedDirectories, CancellationToken token = default)
    {
        int batchSize = _userConfigurationService.AssetSettings.CatalogBatchSize;
        _currentFolderPath = directory;
        // Explicit refresh for the new folder (not suppressed by the reactive guard)
        SetCataloguedAssetsByPath(_assetRepository.GetCataloguedAssetsByPath(_currentFolderPath));

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

        FileInfo[] fileInfos = _fileOperationsService.GetFileInfos(directory);
        (string[] fileNames, Dictionary<string, FileProperties> filePropertiesByName) = CreateFileSnapshot(fileInfos);

        CatalogNewAssets(directory, callback, ref cataloguedAssetsBatchCount, batchSize, fileNames, token);

        _imageMetadataService.UpdateAssetsFileProperties(_cataloguedAssetsByPath, filePropertiesByName);
        string[] updatedFileNames = _assetsComparator.GetUpdatedFileNames(_cataloguedAssetsByPath);

        CatalogUpdatedAssets(directory, callback, ref cataloguedAssetsBatchCount, batchSize, updatedFileNames, token);

        string[] deletedFileNames = _assetsComparator.GetDeletedFileNames(fileNames, _cataloguedAssetsByPath);

        CatalogDeletedAssets(directory, callback, ref cataloguedAssetsBatchCount, batchSize, deletedFileNames, token);

        token.ThrowIfCancellationRequested();

        if (cataloguedAssetsBatchCount >= batchSize)
        {
            return;
        }

        IEnumerable<DirectoryInfo> subdirectories = new DirectoryInfo(directory).EnumerateDirectories()
            .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase);

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

        int assetIndex = 0;

        while (assetIndex < _cataloguedAssetsByPath.Count)
        {
            token.ThrowIfCancellationRequested();

            if (cataloguedAssetsBatchCount >= batchSize)
            {
                break;
            }

            Asset asset = _cataloguedAssetsByPath[assetIndex];
            _ = _assetRepository.DeleteAsset(directory, asset.FileName);
            _cataloguedAssetsByPath.RemoveAt(assetIndex);
            _backupHasSameContent = false;
            cataloguedAssetsBatchCount++;

            callback(new()
            {
                Asset = asset,
                CataloguedAssetsByPath = _cataloguedAssetsByPathView,
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
    }

    private void CatalogNewAssets(string directory, CatalogChangeCallback callback, ref int cataloguedAssetsBatchCount,
        int batchSize, string[] fileNames, CancellationToken token = default)
    {
        (string[] imageNames, string[] videoNames) = _assetsComparator.GetImageAndVideoNames(fileNames);

        string[] newImageFileNames = _assetsComparator.GetNewFileNames(imageNames, _cataloguedAssetsByPath);
        string[] newVideoFileNames = _assetsComparator.GetNewFileNames(videoNames, _cataloguedAssetsByPath);

        CatalogAssets(newImageFileNames, false, directory, callback, ref cataloguedAssetsBatchCount, batchSize,
            CatalogAssetPipelineOperation.Create, token);

        if (_userConfigurationService.AssetSettings.AnalyseVideos)
        {
            CatalogVideoAssetsSequential(newVideoFileNames, directory, callback, ref cataloguedAssetsBatchCount,
                batchSize, token);
        }
    }

    private void CatalogAssets(string[] fileNames, bool isAssetVideo, string directory,
        CatalogChangeCallback callback, ref int cataloguedAssetsBatchCount, int batchSize,
        CatalogAssetPipelineOperation operation, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        int remainingBatchCount = batchSize - cataloguedAssetsBatchCount;

        if (remainingBatchCount <= 0)
        {
            return;
        }

        if (fileNames.Length == 0)
        {
            return;
        }

        Array.Sort(fileNames, StringComparer.Ordinal);

        int cataloguedCount = _catalogFolderPipeline.CatalogAsync(
                directory,
                fileNames,
                isAssetVideo,
                remainingBatchCount,
                operation,
                (result, ct) => OnCatalogPipelineResultAsync(result, callback, ct),
                token)
            .GetAwaiter()
            .GetResult();

        cataloguedAssetsBatchCount += cataloguedCount;
    }

    private void CatalogVideoAssetsSequential(IEnumerable<string> fileNames, string directory,
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
            Asset? newAsset = _assetCreationService.CreateAsset(directory, fileName, isVideo: true,
                skipCatalogCheck: true);

            if (newAsset == null)
            {
                continue;
            }

            _cataloguedAssetsByPath.Add(newAsset);

            callback(new()
            {
                Asset = newAsset,
                CataloguedAssetsByPath = _cataloguedAssetsByPathView,
                Reason = CatalogChangeReason.AssetCreated,
                Message = $"Image {fullPath} added to catalog."
            });

            _backupHasSameContent = false;
            cataloguedAssetsBatchCount++;
        }
    }

    private ValueTask<bool> OnCatalogPipelineResultAsync(CatalogPipelineResult result,
        CatalogChangeCallback callback, CancellationToken _)
    {
        string fullPath = Path.Combine(result.DirectoryPath, result.FileName);

        if (result.Asset == null)
        {
            if (result.Operation == CatalogAssetPipelineOperation.Update)
            {
                Asset? deletedAsset = _assetRepository.DeleteAsset(result.DirectoryPath, result.FileName);
                _cataloguedAssetsByPath.RemoveAll(a => a.FileName == result.FileName);
                _backupHasSameContent = false;

                if (deletedAsset != null)
                {
                    callback(new()
                    {
                        Asset = deletedAsset,
                        CataloguedAssetsByPath = _cataloguedAssetsByPathView,
                        Reason = CatalogChangeReason.AssetDeleted,
                        Message = $"Image {fullPath} deleted from catalog (corrupted)."
                    });
                }
            }
            else
            {
                callback(new()
                {
                    CataloguedAssetsByPath = _cataloguedAssetsByPathView,
                    Reason = CatalogChangeReason.AssetNotCreated,
                    Message = $"Image {fullPath} not added to catalog (corrupted)."
                });
            }

            return ValueTask.FromResult(false);
        }

        if (result.Operation == CatalogAssetPipelineOperation.Update)
        {
            _cataloguedAssetsByPath.RemoveAll(a => a.FileName == result.FileName);
        }

        _cataloguedAssetsByPath.Add(result.Asset);

        CatalogChangeReason reason = result.Operation == CatalogAssetPipelineOperation.Update
            ? CatalogChangeReason.AssetUpdated
            : CatalogChangeReason.AssetCreated;
        string message = result.Operation == CatalogAssetPipelineOperation.Update
            ? $"Image {fullPath} updated in catalog."
            : $"Image {fullPath} added to catalog.";

        callback(new()
        {
            Asset = result.Asset,
            CataloguedAssetsByPath = _cataloguedAssetsByPathView,
            Reason = reason,
            Message = message
        });

        _backupHasSameContent = false;

        return ValueTask.FromResult(true);
    }

    private void CatalogUpdatedAssets(string directory, CatalogChangeCallback callback,
        ref int cataloguedAssetsBatchCount, int batchSize, string[] updatedFileNames,
        CancellationToken token = default)
    {
        if (updatedFileNames.Length == 0)
        {
            return;
        }

        (string[] imageNames, string[] videoNames) = _assetsComparator.GetImageAndVideoNames(updatedFileNames);

        CatalogAssets(imageNames, false, directory, callback, ref cataloguedAssetsBatchCount, batchSize,
            CatalogAssetPipelineOperation.Update, token);

        if (_userConfigurationService.AssetSettings.AnalyseVideos)
        {
            CatalogUpdatedVideoAssetsSequential(directory, callback, ref cataloguedAssetsBatchCount, batchSize,
                videoNames, token);
        }
    }

    private void CatalogUpdatedVideoAssetsSequential(string directory, CatalogChangeCallback callback,
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

            _ = _assetRepository.DeleteAsset(directory, fileName);
            _cataloguedAssetsByPath.RemoveAll(a => a.FileName == fileName);
            _backupHasSameContent = false;

            Asset? updatedAsset = _assetCreationService.CreateAsset(directory, fileName, isVideo: true,
                skipCatalogCheck: true);

            if (updatedAsset == null)
            {
                continue;
            }

            _cataloguedAssetsByPath.Add(updatedAsset);

            callback(new()
            {
                Asset = updatedAsset,
                CataloguedAssetsByPath = _cataloguedAssetsByPathView,
                Reason = CatalogChangeReason.AssetUpdated,
                Message = $"Image {fullPath} updated in catalog."
            });

            cataloguedAssetsBatchCount++;
        }
    }

    private void CatalogDeletedAssets(string directory, CatalogChangeCallback callback,
        ref int cataloguedAssetsBatchCount, int batchSize, string[] deletedFileNames,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        int remainingBatchCount = batchSize - cataloguedAssetsBatchCount;

        if (remainingBatchCount <= 0)
        {
            return;
        }

        string[] fileNamesToDelete = deletedFileNames.Length <= remainingBatchCount
            ? deletedFileNames
            : CopyTakeToArray(deletedFileNames, remainingBatchCount);

        if (fileNamesToDelete.Length == 0)
        {
            return;
        }

        IReadOnlyList<Asset> deletedAssets = _assetRepository.DeleteAssets(directory, fileNamesToDelete);

        for (int i = 0; i < deletedAssets.Count; i++)
        {
            Asset deletedAsset = deletedAssets[i];
            string fullPath = Path.Combine(directory, deletedAsset.FileName);

            _cataloguedAssetsByPath.RemoveAll(a => a.FileName == deletedAsset.FileName);

            callback(new()
            {
                Asset = deletedAsset,
                CataloguedAssetsByPath = _cataloguedAssetsByPathView,
                Reason = CatalogChangeReason.AssetDeleted,
                Message = $"Image {fullPath} deleted from catalog."
            });
            _backupHasSameContent = false;
            cataloguedAssetsBatchCount++;

            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException(token);
            }
        }
    }

    private (string[] FileNames, Dictionary<string, FileProperties> FilePropertiesByName) CreateFileSnapshot(
        FileInfo[] fileInfos)
    {
        List<string> fileNames = new(fileInfos.Length);
        Dictionary<string, FileProperties> filePropertiesByName = new(fileInfos.Length, StringComparer.Ordinal);

        for (int i = 0; i < fileInfos.Length; i++)
        {
            FileInfo fileInfo = fileInfos[i];

            if (TryCreateFileProperties(fileInfo, out FileProperties fileProperties))
            {
                fileNames.Add(fileInfo.Name);
                filePropertiesByName[fileInfo.Name] = fileProperties;
            }
        }

        return (CopyToArray(fileNames), filePropertiesByName);
    }

    private void SetCataloguedAssetsByPath(Asset[] cataloguedAssets)
    {
        _cataloguedAssetsByPath = new(cataloguedAssets.Length);

        for (int i = 0; i < cataloguedAssets.Length; i++)
        {
            _cataloguedAssetsByPath.Add(cataloguedAssets[i]);
        }

        _cataloguedAssetsByPathView = _cataloguedAssetsByPath.AsReadOnly();
    }

    private static string[] CopyToArray(List<string> values)
    {
        string[] result = new string[values.Count];
        values.CopyTo(result);
        return result;
    }

    private static string[] CopyTakeToArray(string[] values, int count)
    {
        string[] result = new string[count];
        Array.Copy(values, result, count);
        return result;
    }

    private bool TryCreateFileProperties(FileInfo fileInfo, out FileProperties fileProperties)
    {
        try
        {
            fileProperties = new()
            {
                Size = fileInfo.Length,
                Creation = fileInfo.CreationTime,
                Modification = fileInfo.LastWriteTime
            };

            return true;
        }
        catch (IOException ex)
        {
            _logger.LogInformation(ex, "Skipped file metadata snapshot for {FilePath}.", fileInfo.FullName);
        }

        fileProperties = default;
        return false;
    }

    #endregion
}
