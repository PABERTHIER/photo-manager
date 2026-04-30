namespace PhotoManager.Domain.Interfaces;

public interface ICatalogAssetsService
{
    Task CatalogAssetsAsync(CatalogChangeCallback callback, CancellationToken token = default);
}
