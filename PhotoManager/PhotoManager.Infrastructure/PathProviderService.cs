namespace PhotoManager.Infrastructure;

public class PathProviderService : IPathProviderService
{
    private static readonly string DatabaseDirectory = Path.Combine(AppContext.BaseDirectory, "Database");

    public string ResolveDatabaseDirectory()
    {
        return DatabaseDirectory;
    }
}
