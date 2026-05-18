namespace PhotoManager.Domain.Interfaces.Persistence.Repositories;

public interface IFolderPersistence
{
    Folder Insert(string path);
    bool Delete(Guid folderId);
    Folder? GetById(Guid folderId);
    Folder? GetByPath(string path);
    IReadOnlyList<Folder> GetAll();
    int Count();
}
