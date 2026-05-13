namespace PhotoManager.Persistence.Repositories;

public interface IRecentPathsPersistence
{
    IReadOnlyList<string> GetAll();
    void Replace(IReadOnlyList<string> paths);
}
