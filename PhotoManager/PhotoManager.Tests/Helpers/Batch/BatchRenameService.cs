using PhotoManager.Domain;
using PhotoManager.Domain.Interfaces;

namespace PhotoManager.Tests.Helpers.Batch;

public class BatchRenameService : IBatchRenameService
{
    private readonly IStorageService _storageService;
    private readonly IMoveAssetsService _moveAssetsService;

    public BatchRenameService(IStorageService storageService, IMoveAssetsService moveAssetsService)
    {
        _storageService = storageService;
        _moveAssetsService = moveAssetsService;
    }

    public BatchRenameResult BatchRename(Asset[] sourceAssets, string batchFormat, bool overwriteExistingTargetFiles)
    {
        BatchRenameResult batchRenameResult = new();

        if (BatchHelper.IsValidBatchFormat(batchFormat))
        {
            for (int i = 0; i < sourceAssets.Length; i++)
            {
                string targetPath = BatchHelper.ComputeTargetPath(
                    sourceAssets[i],
                    batchFormat,
                    i + 1,
                    Thread.CurrentThread.CurrentCulture,
                    _storageService,
                    overwriteExistingTargetFiles);

                if (!string.IsNullOrEmpty(targetPath)
                    && _moveAssetsService.MoveImage(sourceAssets[i].FullPath, targetPath))
                {
                    batchRenameResult.SourceAssets.Add(sourceAssets[i]);
                    batchRenameResult.TargetPaths.Add(targetPath);
                }
            }
        }

        return batchRenameResult;
    }
}
