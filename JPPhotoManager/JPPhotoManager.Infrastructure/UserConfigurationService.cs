using JPPhotoManager.Domain;
using JPPhotoManager.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace JPPhotoManager.Infrastructure
{
    public class UserConfigurationService : IUserConfigurationService
    {
        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;
        private const string CATALOG_BATCH_SIZE_KEY = "appsettings:CatalogBatchSize";
        private const string CATALOG_COOLDOWN_MINUTES = "appsettings:CatalogCooldownMinutes";
        private const string BACKUPS_TO_KEEP = "appsettings:BackupsToKeep";
        private const string THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP = "appsettings:ThumbnailsDictionaryEntriesToKeep";
        private const string REPOSITORY_OWNER = "appsettings:Repository:Owner";
        private const string REPOSITORY_NAME = "appsettings:Repository:Name";

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private readonly IConfigurationRoot configuration;

        public UserConfigurationService(IConfigurationRoot configuration)
        {
            this.configuration = configuration;
        }

        public string GetPicturesDirectory()
        {
            return Constants.PathLocation;
        }

        public void SetAsWallpaper(Asset asset, WallpaperStyle style)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

            switch (style)
            {
                case WallpaperStyle.Fill:
                    key.SetValue(@"WallpaperStyle", "10");
                    key.SetValue(@"TileWallpaper", "0");
                    break;

                case WallpaperStyle.Fit:
                    key.SetValue(@"WallpaperStyle", "6");
                    key.SetValue(@"TileWallpaper", "0");
                    break;

                case WallpaperStyle.Stretch:
                    key.SetValue(@"WallpaperStyle", "2");
                    key.SetValue(@"TileWallpaper", "0");
                    break;

                case WallpaperStyle.Tile:
                    key.SetValue(@"WallpaperStyle", "0");
                    key.SetValue(@"TileWallpaper", "1");
                    break;

                case WallpaperStyle.Center:
                    key.SetValue(@"WallpaperStyle", "0");
                    key.SetValue(@"TileWallpaper", "0");
                    break;

                case WallpaperStyle.Span:
                    key.SetValue(@"WallpaperStyle", "22");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, asset.FullPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }

        public AboutInformation GetAboutInformation(Assembly assembly)
        {
            string product = null;
            string copyright = null;
            string version = "v" + GetProductVersion();
            var attrs = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute));

            if (attrs.SingleOrDefault() is AssemblyProductAttribute assemblyProduct)
            {
                product = assemblyProduct.Product;
            }

            attrs = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute));

            if (attrs.SingleOrDefault() is AssemblyCopyrightAttribute assemblyCopyright)
            {
                copyright = assemblyCopyright.Copyright;
            }

            AboutInformation aboutInformation = new()
            {
                Product = product,
                Author = copyright,
                Version = version
            };

            return aboutInformation;
        }

        private string GetProductVersion()
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(GetType().Assembly.Location);

            return fileVersionInfo.ProductVersion;
        }

        public string GetInitialFolder()
        {
            return Constants.PathLocation;
        }

        public string GetApplicationDataFolder()
        {
            return Constants.PathBackUp;
        }

        public int GetCatalogBatchSize()
        {
            return configuration.GetValue<int>(CATALOG_BATCH_SIZE_KEY);
        }

        public int GetCatalogCooldownMinutes()
        {
            return configuration.GetValue<int>(CATALOG_COOLDOWN_MINUTES);
        }

        public int GetBackupsToKeep()
        {
            return configuration.GetValue<int>(BACKUPS_TO_KEEP);
        }

        public int GetThumbnailsDictionaryEntriesToKeep()
        {
            return configuration.GetValue<int>(THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP);
        }

        public string GetRepositoryOwner()
        {
            return configuration.GetValue<string>(REPOSITORY_OWNER);
        }

        public string GetRepositoryName()
        {
            return configuration.GetValue<string>(REPOSITORY_NAME);
        }

        public string[] GetRootCatalogFolderPaths()
        {
            // TODO: Validate if some of the root folders are not valid or don't exist any longer.
            List<string> rootPaths = new()
            {
                GetPicturesDirectory()
            };

            return rootPaths.ToArray();
        }
    }
}
