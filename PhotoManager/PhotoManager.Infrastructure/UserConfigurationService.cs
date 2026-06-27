using Microsoft.Extensions.Configuration;
using PhotoManager.Domain.Interfaces.Persistence;
using System.Diagnostics;
using System.Globalization;
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
    public UiSettings UiSettings { get; private set; } = null!;
#pragma warning restore IDE0370

    private readonly IConfigurationRoot _configuration;
    private readonly IPersistenceContext _persistenceContext;
    private EditableUserConfiguration? _editableConfiguration;

    public UserConfigurationService(IConfigurationRoot configuration, IPersistenceContext persistenceContext)
    {
        _configuration = configuration;
        _persistenceContext = persistenceContext;

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

    // Always initialized during construction (InitializeConfigValues -> ApplyEditableConfiguration)
    public EditableUserConfiguration GetEditableConfiguration() => _editableConfiguration!;

    public void SaveEditableConfiguration(EditableUserConfiguration configuration)
    {
        ApplyEditableConfiguration(configuration);
        PersistEditableConfiguration(configuration);
    }

    private void InitializeConfigValues()
    {
        // Project settings are intentionally never persisted: they always come from appsettings.json.
        ProjectSettings = ReadProjectSettings();

        // Persisted settings (if any) live in the SQLite Configuration table and fully override appsettings.json.
        // Absence of any stored row means first run: seed the table from appsettings.json.
        IReadOnlyDictionary<string, string> storedValues = _persistenceContext.Configuration.GetAll();
        bool hasPersistedSettings = storedValues.Count > 0;

        EditableUserConfiguration configuration = hasPersistedSettings
            ? ReadEditableConfiguration(key => ReadStoredValue(storedValues, key))
            : ReadEditableConfiguration(ReadConfigurationValue);

        ApplyEditableConfiguration(configuration);

        if (!hasPersistedSettings)
        {
            PersistEditableConfiguration(configuration);
        }
    }

    private void ApplyEditableConfiguration(EditableUserConfiguration configuration)
    {
        string assetsDirectory = ExpandPath(configuration.PathSettings.AssetsDirectory);
        string exemptedFolderPath = ExpandPath(configuration.PathSettings.ExemptedFolderPath);
        string firstFrameVideosPath = Path.Combine(
            assetsDirectory, configuration.PathSettings.FirstFrameVideosFolderName);

        AssetSettings = configuration.AssetSettings;
        HashSettings = configuration.HashSettings;
        PathSettings = new(assetsDirectory, exemptedFolderPath, firstFrameVideosPath);
        StorageSettings = configuration.StorageSettings;
        UiSettings = configuration.UiSettings;

        _editableConfiguration = configuration;
    }

    private static EditableUserConfiguration ReadEditableConfiguration(Func<string, string> readValue)
    {
        AssetSettings assetSettings = new(
            ReadValue<bool>(readValue, UserConfigurationKeys.ANALYSE_VIDEOS),
            readValue(UserConfigurationKeys.ASSET_CORRUPTED_MESSAGE),
            readValue(UserConfigurationKeys.ASSET_ROTATED_MESSAGE),
            ReadValue<int>(readValue, UserConfigurationKeys.CATALOG_BATCH_SIZE),
            ReadValue<ushort>(readValue, UserConfigurationKeys.CATALOG_COOLDOWN_MINUTES),
            ReadValue<ushort>(readValue, UserConfigurationKeys.CORRUPTED_IMAGE_ORIENTATION),
            ReadValue<ushort>(readValue, UserConfigurationKeys.DEFAULT_EXIF_ORIENTATION),
            ReadValue<bool>(readValue, UserConfigurationKeys.DETECT_THUMBNAILS),
            ReadValue<bool>(readValue, UserConfigurationKeys.SYNC_ASSETS_EVERY_X_MINUTES),
            ReadValue<int>(readValue, UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT),
            ReadValue<int>(readValue, UserConfigurationKeys.THUMBNAIL_MAX_WIDTH));

        HashSettings hashSettings = new(
            ReadValue<ushort>(readValue, UserConfigurationKeys.PHASH_THRESHOLD),
            ReadValue<bool>(readValue, UserConfigurationKeys.USING_DHASH),
            ReadValue<bool>(readValue, UserConfigurationKeys.USING_MD5_HASH),
            ReadValue<bool>(readValue, UserConfigurationKeys.USING_PHASH));

        EditablePathSettings pathSettings = new(
            readValue(UserConfigurationKeys.ASSETS_DIRECTORY),
            readValue(UserConfigurationKeys.EXEMPTED_FOLDER_PATH),
            readValue(UserConfigurationKeys.FIRST_FRAME_VIDEOS_FOLDER_NAME));

        StorageSettings storageSettings = new(
            ReadValue<ushort>(readValue, UserConfigurationKeys.BACKUPS_TO_KEEP),
            ReadValue<ushort>(readValue, UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP));

        UiSettings uiSettings = new(readValue(UserConfigurationKeys.THEME_MODE));

        return new(assetSettings, hashSettings, pathSettings, storageSettings, uiSettings);
    }

    private ProjectSettings ReadProjectSettings()
    {
        string projectName = ReadConfigurationValue(UserConfigurationKeys.PROJECT_NAME);
        string projectOwner = ReadConfigurationValue(UserConfigurationKeys.PROJECT_OWNER);

        return new(projectName, projectOwner);
    }

    // Persists the whole editable configuration atomically (single transaction). The Project section is never
    // stored. The presence of any row is what InitializeConfigValues uses to detect "settings already persisted",
    // so writing the complete set in one transaction guarantees the table is never left partially populated.
    private void PersistEditableConfiguration(EditableUserConfiguration configuration)
    {
        Dictionary<string, string> values = new(StringComparer.Ordinal)
        {
            [UserConfigurationKeys.ANALYSE_VIDEOS] = ToStorageValue(configuration.AssetSettings.AnalyseVideos),
            [UserConfigurationKeys.ASSET_CORRUPTED_MESSAGE] = configuration.AssetSettings.CorruptedMessage,
            [UserConfigurationKeys.ASSET_ROTATED_MESSAGE] = configuration.AssetSettings.RotatedMessage,
            [UserConfigurationKeys.CATALOG_BATCH_SIZE] = ToStorageValue(configuration.AssetSettings.CatalogBatchSize),
            [UserConfigurationKeys.CATALOG_COOLDOWN_MINUTES] =
                ToStorageValue(configuration.AssetSettings.CatalogCooldownMinutes),
            [UserConfigurationKeys.CORRUPTED_IMAGE_ORIENTATION] =
                ToStorageValue(configuration.AssetSettings.CorruptedImageOrientation),
            [UserConfigurationKeys.DEFAULT_EXIF_ORIENTATION] =
                ToStorageValue(configuration.AssetSettings.DefaultExifOrientation),
            [UserConfigurationKeys.DETECT_THUMBNAILS] = ToStorageValue(configuration.AssetSettings.DetectThumbnails),
            [UserConfigurationKeys.SYNC_ASSETS_EVERY_X_MINUTES] =
                ToStorageValue(configuration.AssetSettings.SyncAssetsEveryXMinutes),
            [UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT] =
                ToStorageValue(configuration.AssetSettings.ThumbnailMaxHeight),
            [UserConfigurationKeys.THUMBNAIL_MAX_WIDTH] = ToStorageValue(configuration.AssetSettings.ThumbnailMaxWidth),
            [UserConfigurationKeys.PHASH_THRESHOLD] = ToStorageValue(configuration.HashSettings.PHashThreshold),
            [UserConfigurationKeys.USING_DHASH] = ToStorageValue(configuration.HashSettings.UsingDHash),
            [UserConfigurationKeys.USING_MD5_HASH] = ToStorageValue(configuration.HashSettings.UsingMD5Hash),
            [UserConfigurationKeys.USING_PHASH] = ToStorageValue(configuration.HashSettings.UsingPHash),
            [UserConfigurationKeys.ASSETS_DIRECTORY] = configuration.PathSettings.AssetsDirectory,
            [UserConfigurationKeys.EXEMPTED_FOLDER_PATH] =
                ToStorageValue(configuration.PathSettings.ExemptedFolderPath),
            [UserConfigurationKeys.FIRST_FRAME_VIDEOS_FOLDER_NAME] =
                configuration.PathSettings.FirstFrameVideosFolderName,
            [UserConfigurationKeys.BACKUPS_TO_KEEP] = ToStorageValue(configuration.StorageSettings.BackupsToKeep),
            [UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP] =
                ToStorageValue(configuration.StorageSettings.ThumbnailsDictionaryEntriesToKeep),
            [UserConfigurationKeys.THEME_MODE] = configuration.UiSettings.ThemeMode
        };

        _persistenceContext.Configuration.SetValues(values);
    }

    private static string ToStorageValue(object value)
    {
        return Convert.ToString(value, CultureInfo.InvariantCulture)!;
    }

    private static string ReadStoredValue(IReadOnlyDictionary<string, string> storedValues, string key)
    {
        return storedValues.TryGetValue(key, out string? value)
            ? value
            : throw new InvalidOperationException($"Persisted configuration key '{key}' is missing.");
    }

    private string ReadConfigurationValue(string key)
    {
        return _configuration.GetValue<string>(key)
            ?? throw new InvalidOperationException($"Configuration key '{key}' is missing.");
    }

    private static T ReadValue<T>(Func<string, string> readValue, string key)
    {
        string value = readValue(key);
        return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
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
