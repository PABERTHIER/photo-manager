namespace PhotoManager.Domain.UserConfigurationSettings;

public record class AssetSettings(
    bool AnalyseVideos,
    string AssetCorruptedMessage,
    string AssetRotatedMessage,
    int CatalogBatchSize,
    ushort CatalogCooldownMinutes,
    ushort CorruptedImageOrientation,
    ushort DefaultExifOrientation,
    bool DetectThumbnails,
    bool SyncAssetsEveryXMinutes,
    double ThumbnailMaxHeight,
    double ThumbnailMaxWidth);
