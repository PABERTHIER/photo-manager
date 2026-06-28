namespace PhotoManager.Domain.Models;

public sealed record AssetWithThumbnail(Asset Asset, byte[] ThumbnailData);
