using System.Reflection;

namespace PhotoManager.Domain.Interfaces;

public interface IUserConfigurationService
{
    public AssetSettings AssetSettings { get; }
    public HashSettings HashSettings { get; }
    public PathSettings PathSettings { get; }
    public ProjectSettings ProjectSettings { get; }
    public StorageSettings StorageSettings { get; }

    void SetAsWallpaper(Asset asset, WallpaperStyle style);
    AboutInformation GetAboutInformation(Assembly assembly);
    string[] GetRootCatalogFolderPaths();
}
