namespace PhotoManager.Domain.Interfaces;

public interface IMoveAssetsService
{
    bool MoveAssets(Asset[] assets, Folder destinationFolder, bool preserveOriginalFile);
    void DeleteAssets(Asset[] assets, bool saveCatalog);
    bool CopyAsset(string sourceFilePath, string destinationFilePath);
}
