namespace PhotoManager.Domain;

public class SyncAssetsService(
    IAssetRepository assetRepository,
    IFileOperationsService fileOperationsService,
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
                Execute(definition, callback, result);
            }

            return result;
        });
    }

    private void Execute(
        SyncAssetsDirectoriesDefinition definition,
        ProcessStatusChangedCallback callback,
        List<SyncAssetsResult> result)
    {
        SyncAssetsResult syncAssetsResult = new()
        {
            SourceDirectory = definition.SourceDirectory,
            DestinationDirectory = definition.DestinationDirectory
        };

        if (!fileOperationsService.FolderExists(definition.SourceDirectory))
        {
            syncAssetsResult.Message = $"Source directory '{definition.SourceDirectory}' not found.";
            result.Add(syncAssetsResult);
        }
        else
        {
            try
            {
                if (!fileOperationsService.FolderExists(definition.DestinationDirectory))
                {
                    fileOperationsService.CreateDirectory(definition.DestinationDirectory);
                }

                string[] sourceFileNames = fileOperationsService.GetFileNames(definition.SourceDirectory);
                string[] destinationFileNames = fileOperationsService.GetFileNames(definition.DestinationDirectory);
                string[] newFileNames = assetsComparator.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);
                newFileNames = GetFilesNotAlreadyInDestinationSubDirectories(newFileNames, definition.DestinationDirectory);

                foreach (string newFileName in newFileNames)
                {
                    string sourceFilePath = Path.Combine(definition.SourceDirectory, newFileName);
                    string destinationFilePath = Path.Combine(definition.DestinationDirectory, newFileName);

                    if (moveAssetsService.CopyAsset(sourceFilePath, destinationFilePath))
                    {
                        syncAssetsResult.SyncedImages++;
                        callback(new() { NewStatus = $"'{sourceFilePath}' => '{destinationFilePath}'" });
                    }
                }

                if (definition.DeleteAssetsNotInSource)
                {
                    string[] deletedFileNames = assetsComparator.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

                    foreach (string deletedImage in deletedFileNames)
                    {
                        string destinationPath = Path.Combine(definition.DestinationDirectory, deletedImage);
                        fileOperationsService.DeleteFile(definition.DestinationDirectory, deletedImage);
                        syncAssetsResult.SyncedImages++;
                        callback(new() { NewStatus = $"Deleted '{destinationPath}'" });
                    }
                }

                syncAssetsResult.Message = syncAssetsResult.SyncedImages switch
                {
                    0 => $"No images synced from '{definition.SourceDirectory}' to '{definition.DestinationDirectory}'.",
                    1 => $"{syncAssetsResult.SyncedImages} image synced from '{definition.SourceDirectory}' to '{definition.DestinationDirectory}'.",
                    _ => $"{syncAssetsResult.SyncedImages} images synced from '{definition.SourceDirectory}' to '{definition.DestinationDirectory}'.",
                };

                result.Add(syncAssetsResult);

                if (definition.IncludeSubFolders)
                {
                    DirectoryInfo[] subdirectories = fileOperationsService.GetSubDirectories(definition.SourceDirectory);

                    for (int i = 0; i < subdirectories.Length; i++)
                    {
                        SyncAssetsDirectoriesDefinition subDefinition = new()
                        {
                            SourceDirectory = subdirectories[i].FullName,
                            DestinationDirectory = Path.Combine(definition.DestinationDirectory, subdirectories[i].Name),
                            IncludeSubFolders = definition.IncludeSubFolders,
                            DeleteAssetsNotInSource = definition.DeleteAssetsNotInSource
                        };

                        Execute(subDefinition, callback, result);
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
        DirectoryInfo[] destinationSubDirectories = fileOperationsService.GetRecursiveSubDirectories(destinationDirectory);

        for (int i = 0; i < destinationSubDirectories.Length; i++)
        {
            string[] destinationFileNames = fileOperationsService.GetFileNames(destinationSubDirectories[i].FullName);
            newFileNames = assetsComparator.GetNewFileNamesToSync(newFileNames, destinationFileNames);
        }

        return newFileNames;
    }
}
