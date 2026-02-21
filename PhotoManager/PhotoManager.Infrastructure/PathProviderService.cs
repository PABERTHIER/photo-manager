using System.Runtime.CompilerServices;

namespace PhotoManager.Infrastructure;

public class PathProviderService(IUserConfigurationService userConfigurationService) : IPathProviderService
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ResolveDataDirectory(string storageVersion)
    {
        return Path.Combine(userConfigurationService.PathSettings.BackupPath, $"v{storageVersion}");
    }
}
