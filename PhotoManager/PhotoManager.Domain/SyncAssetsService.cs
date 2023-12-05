namespace PhotoManager.Domain;

public class SyncAssetsService(
    IAssetRepository assetRepository,
    IStorageService storageService,
    IDirectoryComparer directoryComparer,
    IMoveAssetsService moveAssetsService) : ISyncAssetsService
{
    private readonly IAssetRepository _assetRepository = assetRepository;
    private readonly IStorageService _storageService = storageService;
    private readonly IDirectoryComparer _directoryComparer = directoryComparer;
    private readonly IMoveAssetsService _moveAssetsService = moveAssetsService;

    public async Task<List<SyncAssetsResult>> ExecuteAsync(ProcessStatusChangedCallback callback)
    {
        return await Task.Run(() =>
        {
            List<SyncAssetsResult> result = [];
            SyncAssetsConfiguration configuration = _assetRepository.GetSyncAssetsConfiguration();

            foreach (SyncAssetsDirectoriesDefinition definition in configuration.Definitions)
            {
                Execute(definition.SourceDirectory,
                    definition.DestinationDirectory,
                    definition.IncludeSubFolders,
                    definition.DeleteAssetsNotInSource,
                    callback,
                    result);
            }

            return result;
        });
    }

    private void Execute(
        string sourceDirectory,
        string destinationDirectory,
        bool includeSubFolders,
        bool deleteAssetsNotInSource,
        ProcessStatusChangedCallback callback,
        List<SyncAssetsResult> result)
    {
        SyncAssetsResult syncAssetsResult = new()
        {
            SourceDirectory = sourceDirectory,
            DestinationDirectory = destinationDirectory
        };

        if (!_storageService.FolderExists(sourceDirectory))
        {
            syncAssetsResult.Message = $"Source directory '{sourceDirectory}' not found.";
            result.Add(syncAssetsResult);
        }
        else
        {
            try
            {
                if (!_storageService.FolderExists(destinationDirectory))
                {
                    _storageService.CreateDirectory(destinationDirectory);
                }

                string[] sourceFileNames = _storageService.GetFileNames(sourceDirectory);
                string[] destinationFileNames = _storageService.GetFileNames(destinationDirectory);
                string[] newFileNames = _directoryComparer.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);
                newFileNames = GetFilesNotAlreadyInDestinationSubDirectories(newFileNames, destinationDirectory);

                foreach (string newFileName in newFileNames)
                {
                    string sourceFilePath = Path.Combine(sourceDirectory, newFileName);
                    string destinationFilePath = Path.Combine(destinationDirectory, newFileName);

                    if (_moveAssetsService.CopyAsset(sourceFilePath, destinationFilePath))
                    {
                        syncAssetsResult.SyncedImages++;
                        callback(new ProcessStatusChangedCallbackEventArgs { NewStatus = $"'{sourceFilePath}' => '{destinationFilePath}'" });
                    }
                }

                if (deleteAssetsNotInSource)
                {
                    string[] deletedFileNames = _directoryComparer.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

                    foreach (string deletedImage in deletedFileNames)
                    {
                        string destinationPath = Path.Combine(destinationDirectory, deletedImage);
                        _storageService.DeleteFile(destinationDirectory, deletedImage);
                        syncAssetsResult.SyncedImages++;
                        callback(new ProcessStatusChangedCallbackEventArgs { NewStatus = $"Deleted '{destinationPath}'" });
                    }
                }

                syncAssetsResult.Message = syncAssetsResult.SyncedImages switch
                {
                    0 => $"No images synced from '{sourceDirectory}' to '{destinationDirectory}'.",
                    1 => $"{syncAssetsResult.SyncedImages} image synced from '{sourceDirectory}' to '{destinationDirectory}'.",
                    _ => $"{syncAssetsResult.SyncedImages} images synced from '{sourceDirectory}' to '{destinationDirectory}'.",
                };

                result.Add(syncAssetsResult);

                if (includeSubFolders)
                {
                    List<DirectoryInfo> subdirectories = _storageService.GetSubDirectories(sourceDirectory);

                    if (subdirectories != null)
                    {
                        foreach (DirectoryInfo subdir in subdirectories)
                        {
                            Execute(subdir.FullName,
                                Path.Combine(destinationDirectory, subdir.Name),
                                includeSubFolders,
                                deleteAssetsNotInSource,
                                callback,
                                result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                syncAssetsResult.Message = ex.Message;
                result.Add(syncAssetsResult);
            }
        }
    }

    private string[] GetFilesNotAlreadyInDestinationSubDirectories(string[] newFileNames, string destinationDirectory)
    {
        List<DirectoryInfo> destinationSubDirectories = _storageService.GetRecursiveSubDirectories(destinationDirectory);

        if (destinationSubDirectories != null)
        {
            foreach (DirectoryInfo dir in destinationSubDirectories)
            {
                string[] destinationFileNames = _storageService.GetFileNames(dir.FullName);
                newFileNames = _directoryComparer.GetNewFileNamesToSync(newFileNames, destinationFileNames);
            }
        }

        return newFileNames;
    }
}
