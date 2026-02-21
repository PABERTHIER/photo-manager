namespace PhotoManager.Infrastructure;

public class PathProviderService(IUserConfigurationService userConfigurationService) : IPathProviderService
{
    private readonly string _dataDirectory = Path.Combine(
        userConfigurationService.PathSettings.BackupPath,
        $"v{userConfigurationService.StorageSettings.StorageVersion}");

    public string ResolveDataDirectory()
    {
        return _dataDirectory;
    }
}
