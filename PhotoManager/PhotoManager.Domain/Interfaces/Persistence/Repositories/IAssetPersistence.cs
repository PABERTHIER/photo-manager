namespace PhotoManager.Domain.Interfaces.Persistence.Repositories;

public interface IAssetPersistence
{
    void Upsert(Asset asset);
    void UpsertMany(IReadOnlyList<Asset> assets);
    bool Delete(Guid folderId, string fileName);
    int DeleteByFolderId(Guid folderId);
    Asset? Get(Guid folderId, string fileName);
    IReadOnlyList<Asset> GetByFolderId(Guid folderId);
    IReadOnlyList<Asset> GetAll();
    IReadOnlyList<Asset> GetByHash(string hash);
    int Count();
}
