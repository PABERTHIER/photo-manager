namespace PhotoManager.Domain.Interfaces;

public interface IAssetCreationService
{
    Asset? CreateAsset(string directoryName, string fileName, bool isVideo = false);
}