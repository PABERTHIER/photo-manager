namespace PhotoManager.Domain;

public sealed record AssetWithThumbnail(Asset Asset, byte[] ThumbnailData);
