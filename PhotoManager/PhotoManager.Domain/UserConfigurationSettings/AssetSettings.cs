namespace PhotoManager.Domain.UserConfigurationSettings;

public record AssetSettings(
    bool AnalyseVideos,
    string CorruptedMessage,
    string RotatedMessage,
    int CatalogBatchSize,
    ushort CatalogCooldownMinutes,
    ushort CorruptedImageOrientation,
    ushort DefaultExifOrientation,
    bool DetectThumbnails,
    bool SyncAssetsEveryXMinutes,
    int ThumbnailMaxHeight,
    int ThumbnailMaxWidth);
