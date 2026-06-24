namespace PhotoManager.Domain;

public delegate void AssetConversionProgressCallback(AssetConversionProgressEventArgs e);

public class AssetConversionProgressEventArgs
{
    public required int CompletedCount { get; init; }
    public required int TotalCount { get; init; }
    public required string Message { get; init; }

    public double ProgressPercentage => TotalCount == 0 ? 0 : CompletedCount * 100d / TotalCount;
}
