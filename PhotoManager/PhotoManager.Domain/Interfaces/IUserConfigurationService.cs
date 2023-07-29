using System.Reflection;

namespace PhotoManager.Domain.Interfaces;

public interface IUserConfigurationService
{
    void SetAsWallpaper(Asset asset, WallpaperStyle style);
    AboutInformation GetAboutInformation(Assembly assembly);
    string GetPicturesDirectory();
    string GetApplicationBackupFolder();
    int GetCatalogBatchSize();
    int GetCatalogCooldownMinutes();
    int GetBackupsToKeep();
    int GetThumbnailsDictionaryEntriesToKeep();
    string[] GetRootCatalogFolderPaths();
}
