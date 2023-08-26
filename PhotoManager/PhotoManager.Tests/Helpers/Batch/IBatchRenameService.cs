namespace PhotoManager.Tests.Helpers.Batch;

public interface IBatchRenameService
{
    BatchRenameResult BatchRename(Asset[] sourceAssets, string batchFormat, bool overwriteExistingTargetFiles);
}
