namespace PhotoManager.Domain;

public delegate void CatalogChangeCallback(CatalogChangeCallbackEventArgs e);

public class CatalogChangeCallbackEventArgs
{
    private static readonly IReadOnlyList<Asset> EmptyCataloguedAssetsByPath = [];

    public Asset? Asset { get; init; }
    public Folder? Folder { get; init; }
    public IReadOnlyList<Asset> CataloguedAssetsByPath { get; init; } = EmptyCataloguedAssetsByPath;
    public required CatalogChangeReason Reason { get; init; }
    public required string Message { get; init; }
    public Exception? Exception { get; init; }
}
