using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PhotoManager.Infrastructure;

public partial class UserConfigurationService : IUserConfigurationService
{
#pragma warning disable IDE0370
    public AssetSettings AssetSettings { get; private set; } = null!;
    public HashSettings HashSettings { get; private set; } = null!;
    public PathSettings PathSettings { get; private set; } = null!;
    public ProjectSettings ProjectSettings { get; private set; } = null!;
    public StorageSettings StorageSettings { get; private set; } = null!;
#pragma warning restore IDE0370

    private readonly IConfigurationRoot _configuration;

    public UserConfigurationService(IConfigurationRoot configuration)
    {
        _configuration = configuration;

        InitializeConfigValues();
    }

    public AboutInformation GetAboutInformation(Assembly assembly)
    {
        string? product = null;
        string version = "v" + GetProductVersion();
        IEnumerable<Attribute> attrs = assembly.GetCustomAttributes<AssemblyProductAttribute>();

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
        return [PathSettings.AssetsDirectory];
    }

    private void InitializeConfigValues()
    {
        bool analyseVideos = _configuration.GetValue<bool>(UserConfigurationKeys.ANALYSE_VIDEOS);
        string? corruptedMessage = _configuration.GetValue<string>(UserConfigurationKeys.ASSET_CORRUPTED_MESSAGE);
        string? rotatedMessage = _configuration.GetValue<string>(UserConfigurationKeys.ASSET_ROTATED_MESSAGE);
        int catalogBatchSize = _configuration.GetValue<int>(UserConfigurationKeys.CATALOG_BATCH_SIZE);
        ushort catalogCooldownMinutes = _configuration.GetValue<ushort>(UserConfigurationKeys.CATALOG_COOLDOWN_MINUTES);
        ushort corruptedImageOrientation =
            _configuration.GetValue<ushort>(UserConfigurationKeys.CORRUPTED_IMAGE_ORIENTATION);
        ushort defaultExifOrientation = _configuration.GetValue<ushort>(UserConfigurationKeys.DEFAULT_EXIF_ORIENTATION);
        bool detectThumbnails = _configuration.GetValue<bool>(UserConfigurationKeys.DETECT_THUMBNAILS);
        bool syncAssetsEveryXMinutes = _configuration.GetValue<bool>(UserConfigurationKeys.SYNC_ASSETS_EVERY_X_MINUTES);
        int thumbnailMaxHeight = _configuration.GetValue<int>(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT);
        int thumbnailMaxWidth = _configuration.GetValue<int>(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH);

        AssetSettings = new(
            analyseVideos,
            corruptedMessage!,
            rotatedMessage!,
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

        HashSettings = new(pHashThreshold, usingDHash, usingMD5Hash, usingPHash);

        string assetsDirectory = ExpandPath(_configuration.GetValue<string>(UserConfigurationKeys.ASSETS_DIRECTORY)!);
        string exemptedFolderPath =
            ExpandPath(_configuration.GetValue<string>(UserConfigurationKeys.EXEMPTED_FOLDER_PATH)!);
        string? firstFrameVideosFolderName =
            _configuration.GetValue<string>(UserConfigurationKeys.FIRST_FRAME_VIDEOS_FOLDER_NAME);

        string firstFrameVideosPath = Path.Combine(assetsDirectory, firstFrameVideosFolderName!);

        PathSettings = new(assetsDirectory, exemptedFolderPath, firstFrameVideosPath);

        string? projectName = _configuration.GetValue<string>(UserConfigurationKeys.PROJECT_NAME);
        string? projectOwner = _configuration.GetValue<string>(UserConfigurationKeys.PROJECT_OWNER);

        ProjectSettings = new(projectName!, projectOwner!);

        ushort backupsToKeep = _configuration.GetValue<ushort>(UserConfigurationKeys.BACKUPS_TO_KEEP);
        ushort thumbnailsDictionaryEntriesToKeep =
            _configuration.GetValue<ushort>(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP);

        StorageSettings = new(backupsToKeep, thumbnailsDictionaryEntriesToKeep);
    }

    private string? GetProductVersion()
    {
        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(GetType().Assembly.Location);

        return fileVersionInfo.ProductVersion;
    }

    private static string ExpandPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string expandedPath = ExpandHomeDirectory(path, homeDirectory);
        expandedPath = Environment.ExpandEnvironmentVariables(expandedPath);

        expandedPath = UnixEnvironmentVariableRegex().Replace(expandedPath, match =>
        {
            string variableName = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
            string? value = Environment.GetEnvironmentVariable(variableName);

            return value ?? match.Value;
        });

        return Path.GetFullPath(expandedPath);
    }

    public static string ExpandHomeDirectory(string path, string homeDirectory)
    {
        if (path != "~"
            && !path.StartsWith("~/", StringComparison.Ordinal)
            && !path.StartsWith("~\\", StringComparison.Ordinal))
        {
            return path;
        }

        if (string.IsNullOrWhiteSpace(homeDirectory))
        {
            return path;
        }

        return path == "~" ? homeDirectory : Path.Combine(homeDirectory, path[2..]);
    }

    [GeneratedRegex(@"\$(\w+)|\$\{([^}]+)\}")]
    private static partial Regex UnixEnvironmentVariableRegex();
}
