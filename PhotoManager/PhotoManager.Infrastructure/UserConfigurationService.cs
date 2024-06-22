using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PhotoManager.Infrastructure;

public class UserConfigurationService : IUserConfigurationService
{
    private const int SPI_SETDESKWALLPAPER = 20;
    private const int SPIF_UPDATEINIFILE = 0x01;
    private const int SPIF_SENDWININICHANGE = 0x02;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    public AssetSettings AssetSettings { get; private set; } = default!;
    public HashSettings HashSettings { get; private set; } = default!;
    public PathSettings PathSettings { get; private set; } = default!;
    public ProjectSettings ProjectSettings { get; private set; } = default!;
    public StorageSettings StorageSettings { get; private set; } = default!;

    private readonly IConfigurationRoot _configuration;

    public UserConfigurationService(IConfigurationRoot configuration)
    {
        _configuration = configuration;

        InitializeConfigValues();
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
            _ = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, asset.FullPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }

    public AboutInformation GetAboutInformation(Assembly assembly)
    {
        string? product = null;
        string version = "v" + GetProductVersion();
        IEnumerable<Attribute> attrs = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute));

        if (attrs.SingleOrDefault() is AssemblyProductAttribute assemblyProduct)
        {
            product = assemblyProduct.Product;
        }

        AboutInformation aboutInformation = new()
        {
            Product = product ?? ProjectSettings.Name,
            Author = ProjectSettings.Owner,
            Version = version
        };

        return aboutInformation;
    }

    public string[] GetRootCatalogFolderPaths()
    {
        List<string> rootPaths = new()
        {
            PathSettings.AssetsDirectory
        };

        return rootPaths.ToArray();
    }

    private void InitializeConfigValues()
    {
        bool analyseVideos = _configuration.GetValue<bool>(UserConfigurationKeys.ANALYSE_VIDEOS);
        string? assetCorruptedMessage = _configuration.GetValue<string>(UserConfigurationKeys.ASSET_CORRUPTED_MESSAGE);
        string? assetRotatedMessage = _configuration.GetValue<string>(UserConfigurationKeys.ASSET_ROTATED_MESSAGE);
        int catalogBatchSize = _configuration.GetValue<int>(UserConfigurationKeys.CATALOG_BATCH_SIZE);
        ushort catalogCooldownMinutes = _configuration.GetValue<ushort>(UserConfigurationKeys.CATALOG_COOLDOWN_MINUTES);
        ushort corruptedImageOrientation = _configuration.GetValue<ushort>(UserConfigurationKeys.CORRUPTED_IMAGE_ORIENTATION);
        ushort defaultExifOrientation = _configuration.GetValue<ushort>(UserConfigurationKeys.DEFAULT_EXIF_ORIENTATION);
        bool detectThumbnails = _configuration.GetValue<bool>(UserConfigurationKeys.DETECT_THUMBNAILS);
        bool syncAssetsEveryXMinutes = _configuration.GetValue<bool>(UserConfigurationKeys.SYNC_ASSETS_EVERY_X_MINUTES);
        int thumbnailMaxHeight = _configuration.GetValue<int>(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT);
        int thumbnailMaxWidth = _configuration.GetValue<int>(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH);

        AssetSettings = new (
            analyseVideos,
            assetCorruptedMessage!,
            assetRotatedMessage!,
            catalogBatchSize,
            catalogCooldownMinutes,
            corruptedImageOrientation,
            defaultExifOrientation,
            detectThumbnails,
            syncAssetsEveryXMinutes,
            thumbnailMaxHeight,
            thumbnailMaxWidth);

        ushort pHashThreshold = _configuration.GetValue<ushort>(UserConfigurationKeys.PHASH_THRESHOLD);
        bool usingDHash = _configuration.GetValue<bool>(UserConfigurationKeys.USING_DHASH);
        bool usingMD5Hash = _configuration.GetValue<bool>(UserConfigurationKeys.USING_MD5_HASH);
        bool usingPHash = _configuration.GetValue<bool>(UserConfigurationKeys.USING_PHASH);

        HashSettings = new (pHashThreshold, usingDHash, usingMD5Hash, usingPHash);

        string? assetsDirectory = _configuration.GetValue<string>(UserConfigurationKeys.ASSETS_DIRECTORY);
        string? backupPath = _configuration.GetValue<string>(UserConfigurationKeys.BACKUP_PATH);
        string? exemptedFolderPath = _configuration.GetValue<string>(UserConfigurationKeys.EXEMPTED_FOLDER_PATH);
        string? ffmpegPath = _configuration.GetValue<string>(UserConfigurationKeys.FFMPEG_PATH);
        string? firstFrameVideosFolderName = _configuration.GetValue<string>(UserConfigurationKeys.FIRST_FRAME_VIDEOS_FOLDER_NAME);

        string firstFrameVideosPath = Path.Combine(assetsDirectory!, firstFrameVideosFolderName!);

        PathSettings = new (assetsDirectory!, backupPath!, exemptedFolderPath!, ffmpegPath!, firstFrameVideosPath);

        string? projectName = _configuration.GetValue<string>(UserConfigurationKeys.PROJECT_NAME);
        string? projectOwner = _configuration.GetValue<string>(UserConfigurationKeys.PROJECT_OWNER);

        ProjectSettings = new (projectName!, projectOwner!);

        ushort backupsToKeep = _configuration.GetValue<ushort>(UserConfigurationKeys.BACKUPS_TO_KEEP);

        string? folderNameBlobs = _configuration.GetValue<string>(UserConfigurationKeys.BLOBS_FOLDER_NAME);
        string? folderNameTables = _configuration.GetValue<string>(UserConfigurationKeys.TABLES_FOLDER_NAME);
        FoldersNameSettings foldersNameSettings = new (folderNameBlobs!, folderNameTables!);

        char? separator = _configuration.GetValue<string>(UserConfigurationKeys.SEPARATOR)?.ToCharArray().First();
        double storageVersion = _configuration.GetValue<double>(UserConfigurationKeys.STORAGE_VERSION);

        string? assetsTableName = _configuration.GetValue<string>(UserConfigurationKeys.ASSETS_TABLE_NAME);
        string? foldersTableName = _configuration.GetValue<string>(UserConfigurationKeys.FOLDERS_TABLE_NAME);
        string? recentTargetPathsTableName = _configuration.GetValue<string>(UserConfigurationKeys.RECENT_TARGET_PATHS_TABLE_NAME);
        string? syncAssetsDirectoriesDefinitionsTableName = _configuration.GetValue<string>(UserConfigurationKeys.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_TABLE_NAME);
        TablesSettings tablesSettings = new (assetsTableName!, foldersTableName!, recentTargetPathsTableName!, syncAssetsDirectoriesDefinitionsTableName!);

        ushort thumbnailsDictionaryEntriesToKeep = _configuration.GetValue<ushort>(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP);

        StorageSettings = new (backupsToKeep, foldersNameSettings, (char)separator!, storageVersion, tablesSettings, thumbnailsDictionaryEntriesToKeep);
    }

    private string? GetProductVersion()
    {
        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(GetType().Assembly.Location);

        return fileVersionInfo.ProductVersion;
    }
}
