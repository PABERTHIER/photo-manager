namespace PhotoManager.Domain.Interfaces
{
    public interface IBatchRenameService
    {
        BatchRenameResult BatchRename(Asset[] sourceAssets, string batchFormat, bool overwriteExistingTargetFiles);
    }
}
