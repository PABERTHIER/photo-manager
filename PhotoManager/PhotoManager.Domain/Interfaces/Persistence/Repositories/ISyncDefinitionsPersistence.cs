namespace PhotoManager.Domain.Interfaces.Persistence.Repositories;

public interface ISyncDefinitionsPersistence
{
    IReadOnlyList<SyncAssetsDirectoriesDefinition> GetAll();
    void Replace(IReadOnlyList<SyncAssetsDirectoriesDefinition> definitions);
}
