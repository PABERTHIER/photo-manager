using log4net;
using System.Reflection;

namespace PhotoManager.Domain;

public sealed class CatalogAssetsService: ICatalogAssetsService, IDisposable
{
    private readonly IAssetRepository _assetRepository;
    private readonly IStorageService _storageService;
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
        IStorageService storageService,
        IAssetCreationService assetCreationService,
        IUserConfigurationService userConfigurationService,
        IAssetsComparator assetsComparator)
    {
        _assetRepository = assetRepository;
        _storageService = storageService;
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
                    CatalogFolders(path, callback, ref cataloguedAssetsBatchCount, visitedDirectories, token);
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

        callback?.Invoke(new CatalogChangeCallbackEventArgs
        {
            Folder = folder,
            Message = $"Inspecting folder {directory}.",
            Reason = ReasonEnum.FolderInspectionInProgress
        });

        string[] filesName = _storageService.GetFileNames(directory);

        CatalogNewAssets(directory, callback, ref cataloguedAssetsBatchCount, batchSize, filesName, token);
        CatalogUpdatedAssets(directory, callback, ref cataloguedAssetsBatchCount, batchSize, token);
        CatalogDeletedAssets(directory, callback, ref cataloguedAssetsBatchCount, batchSize, filesName, token);

        bool folderHasThumbnails = folder != null && _assetRepository.FolderHasThumbnails(folder);

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

        if (folder != null)
        {
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

                callback?.Invoke(new CatalogChangeCallbackEventArgs
                {
                    Asset = asset,
                    CataloguedAssetsByPath = _cataloguedAssetsByPath,
                    Message = $"Image {Path.Combine(directory, asset.FileName)} deleted from catalog.",
                    Reason = ReasonEnum.AssetDeleted
                });
            }

            if (_cataloguedAssetsByPath.Count == 0)
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

    private void CatalogAssets(string[] fileNames, bool isAssetVideo, string directory, CatalogChangeCallback? callback, ref int cataloguedAssetsBatchCount, int batchSize, CancellationToken? token = null)
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
                    callback?.Invoke(new CatalogChangeCallbackEventArgs
                    {
                        CataloguedAssetsByPath = _cataloguedAssetsByPath,
                        Message = $"Image {Path.Combine(directory, fileName)} not added to catalog (corrupted).",
                        Reason = ReasonEnum.AssetNotCreated
                    });
                }

                continue;
            }

            // TODO: Reorder each CatalogChangeCallbackEventArgs to match with the class (same for each UT)
            callback?.Invoke(new CatalogChangeCallbackEventArgs
            {
                Asset = newAsset,
                CataloguedAssetsByPath = _cataloguedAssetsByPath,
                Message = $"Image {Path.Combine(directory, newAsset.FileName)} added to catalog.",
                Reason = ReasonEnum.AssetCreated
            });

            _backupHasSameContent = false;
            cataloguedAssetsBatchCount++;
        }
    }

    private void CatalogUpdatedAssets(string directory, CatalogChangeCallback? callback, ref int cataloguedAssetsBatchCount, int batchSize, CancellationToken? token = null)
    {
        // TODO: Need to rework how to update file information
        string[] updatedFileNames = _assetsComparator.GetUpdatedFileNames(_cataloguedAssetsByPath); // TODO: Should not depend on it to have file info for each files -> break content in separate parts

        foreach (string fileName in updatedFileNames)
        {
            // TODO: Only batchSize has been tested, it has to wait the IsCancellationRequested rework to full test the condition
            if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
            {
                break;
            }

            Asset? deletedAsset = _assetRepository.DeleteAsset(directory, fileName);
            _backupHasSameContent = false;
            string fullPath = Path.Combine(directory, fileName);

            if (_storageService.FileExists(fullPath))
            {
                Asset? updatedAsset = _assetCreationService.CreateAsset(directory, fileName);

                if (updatedAsset == null)
                {
                    callback?.Invoke(new CatalogChangeCallbackEventArgs
                    {
                        Asset = deletedAsset,
                        CataloguedAssetsByPath = _cataloguedAssetsByPath,
                        Message = $"Image {Path.Combine(directory, fileName)} deleted from catalog (corrupted).",
                        Reason = ReasonEnum.AssetDeleted
                    });

                    continue;
                }

                // TODO: Move from here and split _assetsComparator.GetUpdatedFileNames usage above !!
                _storageService.LoadFileInformation(updatedAsset);

                callback?.Invoke(new CatalogChangeCallbackEventArgs
                {
                    Asset = updatedAsset,
                    CataloguedAssetsByPath = _cataloguedAssetsByPath,
                    Message = $"Image {fullPath} updated in catalog.",
                    Reason = ReasonEnum.AssetUpdated
                });

                cataloguedAssetsBatchCount++;
            }
        }
    }

    private void CatalogDeletedAssets(string directory, CatalogChangeCallback? callback, ref int cataloguedAssetsBatchCount, int batchSize, string[] fileNames, CancellationToken? token = null)
    {
        string[] deletedFileNames = _assetsComparator.GetDeletedFileNames(fileNames, _cataloguedAssetsByPath);

        foreach (string fileName in deletedFileNames)
        {
            // TODO: Only batchSize has been tested, it has to wait the IsCancellationRequested rework to full test the condition
            if (cataloguedAssetsBatchCount >= batchSize || (token?.IsCancellationRequested ?? false))
            {
                break;
            }

            Asset? deletedAsset = _assetRepository.DeleteAsset(directory, fileName);

            callback?.Invoke(new CatalogChangeCallbackEventArgs
            {
                Asset = deletedAsset,
                CataloguedAssetsByPath = _cataloguedAssetsByPath,
                Message = $"Image {Path.Combine(directory, fileName)} deleted from catalog.",
                Reason = ReasonEnum.AssetDeleted
            });

            _backupHasSameContent = false;
            cataloguedAssetsBatchCount++;
        }
    }

    #endregion
}
