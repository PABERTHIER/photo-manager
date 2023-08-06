using PhotoManager.Domain;

namespace PhotoManager.Tests.Helpers.Batch;

public class BatchRenameResult
{
    public BatchRenameResult()
    {
        SourceAssets = new List<Asset>();
        TargetPaths = new List<string>();
    }

    public List<Asset> SourceAssets { get; set; }
    public List<string> TargetPaths { get; set; }
}
