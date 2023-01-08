using PhotoManager.Domain;

namespace PhotoManager.Tests.Unit.Batch;

public interface IBatchRenameService
{
    BatchRenameResult BatchRename(Asset[] sourceAssets, string batchFormat, bool overwriteExistingTargetFiles);
}
