using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using PhotoManager.Constants;
using PhotoManager.Domain;
using PhotoManager.Domain.Interfaces;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PhotoManager.Infrastructure;

public class UserConfigurationService : IUserConfigurationService
{
    private const int SPI_SETDESKWALLPAPER = 20;
    private const int SPIF_UPDATEINIFILE = 0x01;
    private const int SPIF_SENDWININICHANGE = 0x02;
    private const string CATALOG_BATCH_SIZE_KEY = "appsettings:CatalogBatchSize";
    private const string CATALOG_COOLDOWN_MINUTES = "appsettings:CatalogCooldownMinutes";
    private const string BACKUPS_TO_KEEP = "appsettings:BackupsToKeep";
    private const string THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP = "appsettings:ThumbnailsDictionaryEntriesToKeep";

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    private readonly IConfigurationRoot _configuration;

    public UserConfigurationService(IConfigurationRoot configuration)
    {
        _configuration = configuration;
    }

    public void SetAsWallpaper(Asset asset, WallpaperStyle style)
    {
        RegistryKey? key = Registry.CurrentUser?.OpenSubKey(@"Control Panel\Desktop", true);

        switch (style)
        {
            case WallpaperStyle.Fill:
                key?.SetValue(@"WallpaperStyle", "10");
                key?.SetValue(@"TileWallpaper", "0");
                Console.WriteLine("Wallpaper set for style 10 and tile 0");
                break;

            case WallpaperStyle.Fit:
                key?.SetValue(@"WallpaperStyle", "6");
                key?.SetValue(@"TileWallpaper", "0");
                Console.WriteLine("Wallpaper set for style 6 and tile 0");
                break;

            case WallpaperStyle.Stretch:
                key?.SetValue(@"WallpaperStyle", "2");
                key?.SetValue(@"TileWallpaper", "0");
                Console.WriteLine("Wallpaper set for style 2 and tile 0");
                break;

            case WallpaperStyle.Tile:
                key?.SetValue(@"WallpaperStyle", "0");
                key?.SetValue(@"TileWallpaper", "1");
                Console.WriteLine("Wallpaper set for style 0 and tile 1");
                break;

            case WallpaperStyle.Center:
                key?.SetValue(@"WallpaperStyle", "0");
                key?.SetValue(@"TileWallpaper", "0");
                Console.WriteLine("Wallpaper set for style 0 and tile 0");
                break;

            case WallpaperStyle.Span:
                key?.SetValue(@"WallpaperStyle", "22");
                key?.SetValue(@"TileWallpaper", "0");
                Console.WriteLine("Wallpaper set for style 22 and tile 0");
                break;
        }

        if (File.Exists(asset.FullPath))
        {
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, asset.FullPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }

    public AboutInformation GetAboutInformation(Assembly assembly)
    {
        string? product = null;
        string version = "v" + GetProductVersion();
        var attrs = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute));

        if (attrs.SingleOrDefault() is AssemblyProductAttribute assemblyProduct)
        {
            product = assemblyProduct.Product;
        }

        AboutInformation aboutInformation = new()
        {
            Product = product ?? "",
            Version = version
        };

        return aboutInformation;
    }

    public string GetPicturesDirectory()
    {
        return PathConstants.PathLocation; // old appsettings:InitialDirectory
    }

    public string GetApplicationBackupFolderPath()
    {
        return PathConstants.PathBackup; // old appsettings:ApplicationDataDirectory
    }

    public int GetCatalogBatchSize()
    {
        return _configuration.GetValue<int>(CATALOG_BATCH_SIZE_KEY);
    }

    public int GetCatalogCooldownMinutes()
    {
        return _configuration.GetValue<int>(CATALOG_COOLDOWN_MINUTES);
    }

    public int GetBackupsToKeep()
    {
        return _configuration.GetValue<int>(BACKUPS_TO_KEEP);
    }

    public int GetThumbnailsDictionaryEntriesToKeep()
    {
        return _configuration.GetValue<int>(THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP);
    }

    public string[] GetRootCatalogFolderPaths()
    {
        List<string> rootPaths = new()
        {
            GetPicturesDirectory()
        };

        return rootPaths.ToArray();
    }

    private string? GetProductVersion()
    {
        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(GetType().Assembly.Location);

        return fileVersionInfo.ProductVersion;
    }
}
