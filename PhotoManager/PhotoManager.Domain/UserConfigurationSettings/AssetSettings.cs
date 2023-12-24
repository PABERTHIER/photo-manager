namespace PhotoManager.Domain.UserConfigurationSettings;

public record class AssetSettings(
    bool AnalyseVideos,
    string? AssetCorruptedMessage,
    string? AssetRotatedMessage,
    int CatalogBatchSize,
    ushort CatalogCooldownMinutes,
    ushort DefaultExifOrientation,
    bool DetectThumbnails,
    ushort OrientationCorruptedImage,
    bool SyncAssetsEveryXMinutes,
    double ThumbnailMaxHeight,
    double ThumbnailMaxWidth);
