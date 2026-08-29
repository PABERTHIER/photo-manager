namespace PhotoManager.Infrastructure;

public class PathProviderService : IPathProviderService
{
    public string ResolveDatabaseDirectory()
    {
        string applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (string.IsNullOrWhiteSpace(applicationDataPath))
        {
            applicationDataPath = Path.GetTempPath();
        }

        return Path.Combine(applicationDataPath, "PhotoManager", "Database");
    }
}
