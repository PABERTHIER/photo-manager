﻿namespace PhotoManager.Domain;

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

        if (!storageService.FolderExists(definition.SourceDirectory))
        {
            syncAssetsResult.Message = $"Source directory '{definition.SourceDirectory}' not found.";
            result.Add(syncAssetsResult);
        }
        else
        {
            try
            {
                if (!storageService.FolderExists(definition.DestinationDirectory))
                {
                    storageService.CreateDirectory(definition.DestinationDirectory);
                }

                string[] sourceFileNames = storageService.GetFileNames(definition.SourceDirectory);
                string[] destinationFileNames = storageService.GetFileNames(definition.DestinationDirectory);
                string[] newFileNames = assetsComparator.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);
                newFileNames = GetFilesNotAlreadyInDestinationSubDirectories(newFileNames, definition.DestinationDirectory);

                foreach (string newFileName in newFileNames)
                {
                    string sourceFilePath = Path.Combine(definition.SourceDirectory, newFileName);
                    string destinationFilePath = Path.Combine(definition.DestinationDirectory, newFileName);

                    if (moveAssetsService.CopyAsset(sourceFilePath, destinationFilePath))
                    {
                        syncAssetsResult.SyncedImages++;
                        callback(new ProcessStatusChangedCallbackEventArgs { NewStatus = $"'{sourceFilePath}' => '{destinationFilePath}'" });
                    }
                }

                if (definition.DeleteAssetsNotInSource)
                {
                    string[] deletedFileNames = assetsComparator.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

                    foreach (string deletedImage in deletedFileNames)
                    {
                        string destinationPath = Path.Combine(definition.DestinationDirectory, deletedImage);
                        storageService.DeleteFile(definition.DestinationDirectory, deletedImage);
                        syncAssetsResult.SyncedImages++;
                        callback(new ProcessStatusChangedCallbackEventArgs { NewStatus = $"Deleted '{destinationPath}'" });
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
                    List<DirectoryInfo> subdirectories = storageService.GetSubDirectories(definition.SourceDirectory);

                    for (int i = 0; i < subdirectories.Count; i++)
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
        List<DirectoryInfo> destinationSubDirectories = storageService.GetRecursiveSubDirectories(destinationDirectory);

        for (int i = 0; i < destinationSubDirectories.Count; i++)
        {
            string[] destinationFileNames = storageService.GetFileNames(destinationSubDirectories[i].FullName);
            newFileNames = assetsComparator.GetNewFileNamesToSync(newFileNames, destinationFileNames);
        }

        return newFileNames;
    }
}
