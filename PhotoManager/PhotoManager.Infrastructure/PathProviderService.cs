namespace PhotoManager.Infrastructure;

public class PathProviderService(IUserConfigurationService userConfigurationService) : IPathProviderService
{
    // TODO: Clean up afterward -> Delete the service ?
    private readonly string _databaseDirectory = userConfigurationService.PathSettings.DatabasePath;

    public string ResolveDatabaseDirectory()
    {
        return _databaseDirectory;
    }
}
