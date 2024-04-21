namespace PhotoManager.Domain.Interfaces;

public interface ICatalogAssetsService
{
    Task CatalogAssetsAsync(CatalogChangeCallback? callback, CancellationToken? token = null);
    Asset? CreateAsset(string directoryName, string fileName, bool isVideo = false);
}
