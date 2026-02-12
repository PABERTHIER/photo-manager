using System.Reflection;

namespace PhotoManager.Domain.Interfaces;

public interface IUserConfigurationService
{
    AssetSettings AssetSettings { get; }
    HashSettings HashSettings { get; }
    PathSettings PathSettings { get; }
    ProjectSettings ProjectSettings { get; }
    StorageSettings StorageSettings { get; }

    AboutInformation GetAboutInformation(Assembly assembly);
    string[] GetRootCatalogFolderPaths();
}
