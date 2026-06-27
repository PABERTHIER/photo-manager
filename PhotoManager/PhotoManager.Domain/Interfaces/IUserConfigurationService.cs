using System.Reflection;

namespace PhotoManager.Domain.Interfaces;

public interface IUserConfigurationService
{
    AssetSettings AssetSettings { get; }
    HashSettings HashSettings { get; }
    PathSettings PathSettings { get; }
    PipelineSettings PipelineSettings { get; }
    ProjectSettings ProjectSettings { get; }
    StorageSettings StorageSettings { get; }
    UiSettings UiSettings { get; }

    AboutInformation GetAboutInformation(Assembly assembly);
    string[] GetRootCatalogFolderPaths();
    EditableUserConfiguration GetEditableConfiguration();
    void SaveEditableConfiguration(EditableUserConfiguration configuration);
}
