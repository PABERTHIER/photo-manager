using System.Reflection;

namespace PhotoManager.Domain.Interfaces
{
    public interface IUserConfigurationService
    {
        void SetAsWallpaper(Asset asset, WallpaperStyle style);
        AboutInformation GetAboutInformation(Assembly assembly);
        string GetPicturesDirectory();
        string GetApplicationBackUpFolder();
        int GetCatalogBatchSize();
        int GetCatalogCooldownMinutes();
        int GetBackupsToKeep();
        int GetThumbnailsDictionaryEntriesToKeep();
        string[] GetRootCatalogFolderPaths();
    }
}
