namespace PhotoManager.Infrastructure;

public class PathProviderService : IPathProviderService
{
    private readonly string _databaseDirectory;

    public PathProviderService(IApplicationDataPathProvider applicationDataPathProvider)
    {
        string applicationDataPath = applicationDataPathProvider.GetPath();

        if (string.IsNullOrWhiteSpace(applicationDataPath))
        {
            applicationDataPath = Path.GetTempPath();
        }

        _databaseDirectory = Path.Combine(applicationDataPath, "PhotoManager", "Database");
    }

    public string ResolveDatabaseDirectory()
    {
        return _databaseDirectory;
    }
}
