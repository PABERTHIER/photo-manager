using log4net;
using System.Reflection;

namespace PhotoManager.Domain;

public sealed class CatalogAssetsService : ICatalogAssetsService, IDisposable
{
    private readonly IAssetRepository _assetRepository;
    private readonly IFileOperationsService _fileOperationsService;
    private readonly IImageMetadataService _imageMetadataService;
    private readonly IAssetCreationService _assetCreationService;
    private readonly IUserConfigurationService _userConfigurationService;
    private readonly IAssetsComparator _assetsComparator;
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    private bool _backupHasSameContent = true;
    private string _currentFolderPath = string.Empty;
    private readonly HashSet<string> _directories = [];
    private List<Asset> _cataloguedAssetsByPath = [];
    private readonly IDisposable _assetsUpdatedSubscription;

    public CatalogAssetsService(
        IAssetRepository assetRepository,
        IFileOperationsService fileOperationsService,
        IImageMetadataService imageMetadataService,
        IAssetCreationService assetCreationService,
        IUserConfigurationService userConfigurationService,
        IAssetsComparator assetsComparator)
    {
        _assetRepository = assetRepository;
        _fileOperationsService = fileOperationsService;
        _imageMetadataService = imageMetadataService;
        _assetCreationService = assetCreationService;
        _userConfigurationService = userConfigurationService;
        _assetsComparator = assetsComparator;

        _assetsUpdatedSubscription = assetRepository.AssetsUpdated.Subscribe(_ => UpdateAssets());
    }

    public void Dispose()
    {
        _assetsUpdatedSubscription.Dispose();
    }

    private void UpdateAssets()
    {
        _cataloguedAssetsByPath = _assetRepository.GetCataloguedAssetsByPath(_currentFolderPath);
    }

    public async Task CatalogAssetsAsync(CatalogChangeCallback? callback, CancellationToken? token = null)
    {
        await Task.Run(() =>
        {
            int cataloguedAssetsBatchCount = 0;
            HashSet<string> visitedDirectories = [];

            try
            {
                HashSet<string> foldersPathToCatalog = GetFoldersPathToCatalog();

                foreach (string path in foldersPathToCatalog)
                {
                    // TODO: Rework the whole cancellation
                    // ThrowIfCancellationRequested should be in each if below ?
                    // token?.ThrowIfCancellationRequested();

                    CatalogFolders(path, callback, ref cataloguedAssetsBatchCount, visitedDirectories, token);
                    callback?.Invoke(new() { Reason = CatalogChangeReason.FolderInspectionCompleted, Message = $"Folder inspection for {path}, subfolders included, has been completed." });
                }

                _directories.UnionWith(visitedDirectories);

                if (!_assetRepository.BackupExists() || !_backupHasSameContent)
                {
                    callback?.Invoke(!_assetRepository.BackupExists()
                        ? new() { Reason = CatalogChangeReason.BackupCreationStarted, Message = "Creating catalog backup..." }
                        : new CatalogChangeCallbackEventArgs { Reason = CatalogChangeReason.BackupUpdateStarted, Message = "Updating catalog backup..." });

                    _assetRepository.WriteBackup();
                    callback?.Invoke(new()
                    {
                        Reason = CatalogChangeReason.BackupCompleted,
                        Message = "Backup completed successfully."
                    });

                    _backupHasSameContent = true;
                }
                else
                {
                    callback?.Invoke(new() { Reason = CatalogChangeReason.NoBackupChangesDetected, Message = "No changes made to the backup." });
                }
            }
            catch (OperationCanceledException)
            {
                // If the catalog background process is cancelled, there is a risk that it happens while saving the catalog files.
                // This could result in the files being damaged.
                // Therefore, the application saves the files before the task is completely shut down.

                // TODO: Test if _currentFolderPath is good & SaveCatalog performed correctly
                Folder? currentFolder = _assetRepository.GetFolderByPath(_currentFolderPath);
                _assetRepository.SaveCatalog(currentFolder);

                callback?.Invoke(new()
                {
                    Reason = CatalogChangeReason.CatalogProcessCancelled,
                    Message = "The catalog process has been cancelled."
                });

                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                callback?.Invoke(new()
                {
                    Reason = CatalogChangeReason.CatalogProcessFailed,
                    Message = "The catalog process has failed.",
                    Exception = ex
                });
            }
            finally
            {
                callback?.Invoke(new()
                {
                    Reason = CatalogChangeReason.CatalogProcessEnded,
                    Message = "The catalog process has ended."
                });
            }
        });
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

    private void CatalogFolders(string directory, CatalogChangeCallback? callback, ref int cataloguedAssetsBatchCount, HashSet<string> visitedDirectories, CancellationToken? token = null)
    {
        int batchSize = _userConfigurationService.AssetSettings.CatalogBatchSize;
        _currentFolderPath = directory;
        UpdateAssets(); // Needed when having multiple actions on the same instance

        if (_fileOperationsService.FolderExists(directory))
        {
            CatalogExistingFolder(directory, callback, ref cataloguedAssetsBatchCount, batchSize, visitedDirectories, token);
        }
        else if (!string.IsNullOrEmpty(directory) && !_fileOperationsService.FolderExists(directory))
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

            callback?.Invoke(new()
            {
                Folder = folder,
                Reason = CatalogChangeReason.FolderCreated,
                Message = $"Folder {directory} added to catalog."
            });
        }

        folder = _assetRepository.GetFolderByPath(directory);

        callback?.Invoke(new()
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

        bool isBlobFileExists = folder != null && _assetRepository.IsBlobFileExists(folder.ThumbnailsFilename);

        if (_assetRepository.HasChanges() || !isBlobFileExists)
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
                CatalogFolders(subdirectory.FullName, callback, ref cataloguedAssetsBatchCount, visitedDirectories, token);
            }
        }
    }

    private void CatalogNonExistingFolder(string directory, CatalogChangeCallback? callback, ref int cataloguedAssetsBatchCount, int batchSize, CancellationToken? token = null)
    {
        if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
        {
            return;
        }

        Folder? folder = _assetRepository.GetFolderByPath(directory);

        if (folder == null)
        {
            return;
        }

        foreach (Asset asset in _cataloguedAssetsByPath)
        {
            // TODO: Only batchSize has been tested, it has to wait the IsCancellationRequested rework to full test the condition
            if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
            {
                break;
            }

            _ = _assetRepository.DeleteAsset(directory, asset.FileName);
            _backupHasSameContent = false;
            cataloguedAssetsBatchCount++;

            callback?.Invoke(new()
            {
                Asset = asset,
                CataloguedAssetsByPath = _cataloguedAssetsByPath,
                Reason = CatalogChangeReason.AssetDeleted,
                Message = $"Image {Path.Combine(directory, asset.FileName)} deleted from catalog."
            });
        }

        if (_cataloguedAssetsByPath.Count == 0)
        {
            _assetRepository.DeleteFolder(folder);
            _directories.Remove(folder.Path);

            callback?.Invoke(new()
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

    private void CatalogNewAssets(string directory, CatalogChangeCallback? callback, ref int cataloguedAssetsBatchCount, int batchSize, string[] fileNames, CancellationToken? token = null)
    {
        (string[] imageNames, string[] videoNames) = _assetsComparator.GetImageAndVideoNames(fileNames);

        string[] newImageFileNames = _assetsComparator.GetNewFileNames(imageNames, _cataloguedAssetsByPath);
        string[] newVideoFileNames = _assetsComparator.GetNewFileNames(videoNames, _cataloguedAssetsByPath);

        CatalogAssets(newImageFileNames, false, directory, callback, ref cataloguedAssetsBatchCount, batchSize, token);

        if (_userConfigurationService.AssetSettings.AnalyseVideos)
        {
            CatalogAssets(newVideoFileNames, true, directory, callback, ref cataloguedAssetsBatchCount, batchSize, token);
        }
    }

    private void CatalogAssets(IEnumerable<string> fileNames, bool isAssetVideo, string directory, CatalogChangeCallback? callback, ref int cataloguedAssetsBatchCount, int batchSize, CancellationToken? token = null)
    {
        foreach (string fileName in fileNames)
        {
            if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
            {
                break;
            }

            Asset? newAsset = _assetCreationService.CreateAsset(directory, fileName, isAssetVideo);

            if (newAsset == null)
            {
                if (!isAssetVideo)
                {
                    callback?.Invoke(new()
                    {
                        CataloguedAssetsByPath = _cataloguedAssetsByPath,
                        Reason = CatalogChangeReason.AssetNotCreated,
                        Message = $"Image {Path.Combine(directory, fileName)} not added to catalog (corrupted)."
                    });
                }

                continue;
            }

            callback?.Invoke(new()
            {
                Asset = newAsset,
                CataloguedAssetsByPath = _cataloguedAssetsByPath,
                Reason = CatalogChangeReason.AssetCreated,
                Message = $"Image {Path.Combine(directory, newAsset.FileName)} added to catalog."
            });

            _backupHasSameContent = false;
            cataloguedAssetsBatchCount++;
        }
    }

    private void CatalogUpdatedAssets(string directory, CatalogChangeCallback? callback, ref int cataloguedAssetsBatchCount, int batchSize, IEnumerable<string> updatedFileNames, CancellationToken? token = null)
    {
        foreach (string fileName in updatedFileNames)
        {
            // TODO: Only batchSize has been tested, it has to wait the IsCancellationRequested rework to full test the condition
            if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
            {
                break;
            }

            Asset? deletedAsset = _assetRepository.DeleteAsset(directory, fileName);
            _backupHasSameContent = false;

            Asset? updatedAsset = _assetCreationService.CreateAsset(directory, fileName);

            if (updatedAsset == null)
            {
                callback?.Invoke(new()
                {
                    Asset = deletedAsset,
                    CataloguedAssetsByPath = _cataloguedAssetsByPath,
                    Reason = CatalogChangeReason.AssetDeleted,
                    Message = $"Image {Path.Combine(directory, fileName)} deleted from catalog (corrupted)."
                });

                continue;
            }

            string fullPath = Path.Combine(directory, fileName);
            callback?.Invoke(new()
            {
                Asset = updatedAsset,
                CataloguedAssetsByPath = _cataloguedAssetsByPath,
                Reason = CatalogChangeReason.AssetUpdated,
                Message = $"Image {fullPath} updated in catalog."
            });

            cataloguedAssetsBatchCount++;
        }
    }

    private void CatalogDeletedAssets(string directory, CatalogChangeCallback? callback, ref int cataloguedAssetsBatchCount, int batchSize, IEnumerable<string> deletedFileNames, CancellationToken? token = null)
    {
        foreach (string fileName in deletedFileNames)
        {
            // TODO: Only batchSize has been tested, it has to wait the IsCancellationRequested rework to full test the condition
            if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
            {
                break;
            }

            Asset? deletedAsset = _assetRepository.DeleteAsset(directory, fileName);

            callback?.Invoke(new()
            {
                Asset = deletedAsset,
                CataloguedAssetsByPath = _cataloguedAssetsByPath,
                Reason = CatalogChangeReason.AssetDeleted,
                Message = $"Image {Path.Combine(directory, fileName)} deleted from catalog."
            });

            _backupHasSameContent = false;
            cataloguedAssetsBatchCount++;
        }
    }

    #endregion
}
