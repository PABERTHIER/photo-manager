namespace PhotoManager.Domain;

public class AssetConversionResult
{
    public AssetConversionResult(IReadOnlyList<AssetConversionItemResult> items)
    {
        Items = items;
        ConvertedCount = items.Count(item => item.Success);
        FailedCount = items.Count - ConvertedCount;
    }

    public IReadOnlyList<AssetConversionItemResult> Items { get; }
    public int ConvertedCount { get; }
    public int FailedCount { get; }
}
