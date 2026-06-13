namespace PhotoManager.Domain.Interfaces;

public interface IAssetCreationService
{
    Asset? CreateAsset(string directoryName, string fileName, bool isVideo = false, bool skipCatalogCheck = false);
    AssetWithThumbnail? CreateAssetWithThumbnail(string directoryName, string fileName, byte[] imageBytes,
        bool isVideo = false, bool skipCatalogCheck = false);
}
