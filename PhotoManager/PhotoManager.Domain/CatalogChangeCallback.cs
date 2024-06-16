namespace PhotoManager.Domain;

public delegate void CatalogChangeCallback(CatalogChangeCallbackEventArgs e);

public class CatalogChangeCallbackEventArgs
{
    public Asset? Asset { get; init; }
    public Folder? Folder { get; init; }
    public List<Asset> CataloguedAssetsByPath { get; init; } = [];
    public required ReasonEnum Reason { get; init; }
    public required string Message { get; init; }
    public Exception? Exception { get; init; }
}
