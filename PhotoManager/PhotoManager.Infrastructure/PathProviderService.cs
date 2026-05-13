namespace PhotoManager.Infrastructure;

public class PathProviderService(IUserConfigurationService userConfigurationService) : IPathProviderService
{
    // TODO: Clean up afterward -> Delete the service ?
    private readonly string _dataDirectory = userConfigurationService.PathSettings.BackupPath;

    public string ResolveDataDirectory()
    {
        return _dataDirectory;
    }
}
