namespace PhotoManager.Domain;

public class SyncAssetsService(
    IAssetRepository assetRepository,
    IStorageService storageService,
    IAssetsComparator assetsComparator,
    IMoveAssetsService moveAssetsService) : ISyncAssetsService
{
    public async Task<List<SyncAssetsResult>> ExecuteAsync(ProcessStatusChangedCallback callback)
    {
        return await Task.Run(() =>
        {
            List<SyncAssetsResult> result = [];
            SyncAssetsConfiguration configuration = assetRepository.GetSyncAssetsConfiguration();

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

        if (!storageService.FolderExists(sourceDirectory))
        {
            syncAssetsResult.Message = $"Source directory '{sourceDirectory}' not found.";
            result.Add(syncAssetsResult);
        }
        else
        {
            try
            {
                if (!storageService.FolderExists(destinationDirectory))
                {
                    storageService.CreateDirectory(destinationDirectory);
                }

                string[] sourceFileNames = storageService.GetFileNames(sourceDirectory);
                string[] destinationFileNames = storageService.GetFileNames(destinationDirectory);
                string[] newFileNames = assetsComparator.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);
                newFileNames = GetFilesNotAlreadyInDestinationSubDirectories(newFileNames, destinationDirectory);

                foreach (string newFileName in newFileNames)
                {
                    string sourceFilePath = Path.Combine(sourceDirectory, newFileName);
                    string destinationFilePath = Path.Combine(destinationDirectory, newFileName);

                    if (moveAssetsService.CopyAsset(sourceFilePath, destinationFilePath))
                    {
                        syncAssetsResult.SyncedImages++;
                        callback(new ProcessStatusChangedCallbackEventArgs { NewStatus = $"'{sourceFilePath}' => '{destinationFilePath}'" });
                    }
                }

                if (deleteAssetsNotInSource)
                {
                    string[] deletedFileNames = assetsComparator.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

                    foreach (string deletedImage in deletedFileNames)
                    {
                        string destinationPath = Path.Combine(destinationDirectory, deletedImage);
                        storageService.DeleteFile(destinationDirectory, deletedImage);
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
                    List<DirectoryInfo> subdirectories = storageService.GetSubDirectories(sourceDirectory);

                    for (int i = 0; i < subdirectories.Count; i++)
                    {
                        Execute(subdirectories[i].FullName,
                            Path.Combine(destinationDirectory, subdirectories[i].Name),
                            includeSubFolders,
                            deleteAssetsNotInSource,
                            callback,
                            result);
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
        List<DirectoryInfo> destinationSubDirectories = storageService.GetRecursiveSubDirectories(destinationDirectory);

        for (int i = 0; i < destinationSubDirectories.Count; i++)
        {
            string[] destinationFileNames = storageService.GetFileNames(destinationSubDirectories[i].FullName);
            newFileNames = assetsComparator.GetNewFileNamesToSync(newFileNames, destinationFileNames);
        }

        return newFileNames;
    }
}
