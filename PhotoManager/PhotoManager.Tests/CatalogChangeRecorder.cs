namespace PhotoManager.Tests;

public sealed class CatalogChangeRecorder : List<CatalogChangeCallbackEventArgs>
{
    public new void Add(CatalogChangeCallbackEventArgs catalogChange)
    {
        ArgumentNullException.ThrowIfNull(catalogChange);

        IReadOnlyList<Asset> cataloguedAssetsByPath = catalogChange.CataloguedAssetsByPath;
        Asset[] cataloguedAssetsByPathSnapshot = new Asset[cataloguedAssetsByPath.Count];

        for (int i = 0; i < cataloguedAssetsByPathSnapshot.Length; i++)
        {
            cataloguedAssetsByPathSnapshot[i] = cataloguedAssetsByPath[i];
        }

        base.Add(new()
        {
            Asset = catalogChange.Asset,
            Folder = catalogChange.Folder,
            CataloguedAssetsByPath = cataloguedAssetsByPathSnapshot,
            Reason = catalogChange.Reason,
            Message = catalogChange.Message,
            Exception = catalogChange.Exception
        });
    }
}
