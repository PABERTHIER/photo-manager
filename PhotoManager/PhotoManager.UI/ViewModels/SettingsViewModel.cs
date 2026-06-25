using PhotoManager.Application;
using PhotoManager.Domain.UserConfigurationSettings;

namespace PhotoManager.UI.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly IApplication _application;

    public SettingsViewModel(IApplication application)
    {
        _application = application;
        ThemeModes = ["System", "Light", "Dark"];
        LoadSettings(_application.GetEditableConfiguration());
    }

    public string[] ThemeModes { get; }
    public bool AnalyseVideos { get; set; }
    public string CorruptedMessage { get; set; } = string.Empty;
    public string RotatedMessage { get; set; } = string.Empty;
    public int CatalogBatchSize { get; set; }
    public ushort CatalogCooldownMinutes { get; set; }
    public ushort CorruptedImageOrientation { get; set; }
    public ushort DefaultExifOrientation { get; set; }
    public bool DetectThumbnails { get; set; }
    public bool SyncAssetsEveryXMinutes { get; set; }
    public int ThumbnailMaxHeight { get; set; }
    public int ThumbnailMaxWidth { get; set; }
    public ushort PHashThreshold { get; set; }
    public bool UsingDHash { get; set; }
    public bool UsingMD5Hash { get; set; }
    public bool UsingPHash { get; set; }
    public ushort BackupsToKeep { get; set; }
    public ushort ThumbnailsDictionaryEntriesToKeep { get; set; }

    public string AssetsDirectory
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            NotifyPropertyChanged(nameof(AssetsDirectory));
        }
    }
        = string.Empty;

    public string ExemptedFolderPath
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            NotifyPropertyChanged(nameof(ExemptedFolderPath));
        }
    }
        = string.Empty;

    public string FirstFrameVideosFolderName
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            NotifyPropertyChanged(nameof(FirstFrameVideosFolderName));
        }
    }
        = string.Empty;

    public string ThemeMode
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            NotifyPropertyChanged(nameof(ThemeMode));
        }
    }
        = string.Empty;

    public string StatusMessage
    {
        get;
        private set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            NotifyPropertyChanged(nameof(StatusMessage));
        }
    }
        = string.Empty;

    public bool Save()
    {
        if (!Validate())
        {
            return false;
        }

        EditableUserConfiguration configuration = new(
            new(
                AnalyseVideos,
                CorruptedMessage,
                RotatedMessage,
                CatalogBatchSize,
                CatalogCooldownMinutes,
                CorruptedImageOrientation,
                DefaultExifOrientation,
                DetectThumbnails,
                SyncAssetsEveryXMinutes,
                ThumbnailMaxHeight,
                ThumbnailMaxWidth),
            new(PHashThreshold, UsingDHash, UsingMD5Hash, UsingPHash),
            new(AssetsDirectory, ExemptedFolderPath, FirstFrameVideosFolderName),
            new(BackupsToKeep, ThumbnailsDictionaryEntriesToKeep),
            new(ThemeMode));

        _application.SaveEditableConfiguration(configuration);

        StatusMessage = "Settings saved in the database. They now override appsettings.json values.";

        return true;
    }

    private void LoadSettings(EditableUserConfiguration configuration)
    {
        AnalyseVideos = configuration.AssetSettings.AnalyseVideos;
        CorruptedMessage = configuration.AssetSettings.CorruptedMessage;
        RotatedMessage = configuration.AssetSettings.RotatedMessage;
        CatalogBatchSize = configuration.AssetSettings.CatalogBatchSize;
        CatalogCooldownMinutes = configuration.AssetSettings.CatalogCooldownMinutes;
        CorruptedImageOrientation = configuration.AssetSettings.CorruptedImageOrientation;
        DefaultExifOrientation = configuration.AssetSettings.DefaultExifOrientation;
        DetectThumbnails = configuration.AssetSettings.DetectThumbnails;
        SyncAssetsEveryXMinutes = configuration.AssetSettings.SyncAssetsEveryXMinutes;
        ThumbnailMaxHeight = configuration.AssetSettings.ThumbnailMaxHeight;
        ThumbnailMaxWidth = configuration.AssetSettings.ThumbnailMaxWidth;
        PHashThreshold = configuration.HashSettings.PHashThreshold;
        UsingDHash = configuration.HashSettings.UsingDHash;
        UsingMD5Hash = configuration.HashSettings.UsingMD5Hash;
        UsingPHash = configuration.HashSettings.UsingPHash;
        AssetsDirectory = configuration.PathSettings.AssetsDirectory;
        ExemptedFolderPath = configuration.PathSettings.ExemptedFolderPath;
        FirstFrameVideosFolderName = configuration.PathSettings.FirstFrameVideosFolderName;
        BackupsToKeep = configuration.StorageSettings.BackupsToKeep;
        ThumbnailsDictionaryEntriesToKeep = configuration.StorageSettings.ThumbnailsDictionaryEntriesToKeep;
        ThemeMode = configuration.UiSettings.ThemeMode;
    }

    private bool Validate()
    {
        if (string.IsNullOrWhiteSpace(AssetsDirectory))
        {
            StatusMessage = "Assets directory is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(ExemptedFolderPath))
        {
            StatusMessage = "Exempted folder path is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(FirstFrameVideosFolderName))
        {
            StatusMessage = "First frame videos folder name is required.";
            return false;
        }

        if (CatalogBatchSize <= 0)
        {
            StatusMessage = "Catalog batch size must be greater than 0.";
            return false;
        }

        if (CatalogCooldownMinutes == 0)
        {
            StatusMessage = "Catalog cooldown minutes must be greater than 0.";
            return false;
        }

        if (CorruptedImageOrientation == 0)
        {
            StatusMessage = "Corrupted image orientation must be greater than 0.";
            return false;
        }

        if (DefaultExifOrientation == 0)
        {
            StatusMessage = "Default EXIF orientation must be greater than 0.";
            return false;
        }

        if (ThumbnailMaxHeight <= 0)
        {
            StatusMessage = "Thumbnail max height must be greater than 0.";
            return false;
        }

        if (ThumbnailMaxWidth <= 0)
        {
            StatusMessage = "Thumbnail max width must be greater than 0.";
            return false;
        }

        if (BackupsToKeep == 0)
        {
            StatusMessage = "Backups to keep must be greater than 0.";
            return false;
        }

        if (ThumbnailsDictionaryEntriesToKeep == 0)
        {
            StatusMessage = "Thumbnails dictionary entries to keep must be greater than 0.";
            return false;
        }

        if (!ThemeModes.Contains(ThemeMode, StringComparer.OrdinalIgnoreCase))
        {
            StatusMessage = "Theme mode must be System, Light, or Dark.";
            return false;
        }

        return true;
    }

}
