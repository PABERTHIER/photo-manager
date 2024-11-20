namespace PhotoManager.Domain.UserConfigurationSettings;

public record AssetSettings(
    bool AnalyseVideos,
    string AssetCorruptedMessage,
    string AssetRotatedMessage,
    int CatalogBatchSize,
    ushort CatalogCooldownMinutes,
    ushort CorruptedImageOrientation,
    ushort DefaultExifOrientation,
    bool DetectThumbnails,
    bool SyncAssetsEveryXMinutes,
    int ThumbnailMaxHeight,
    int ThumbnailMaxWidth);
