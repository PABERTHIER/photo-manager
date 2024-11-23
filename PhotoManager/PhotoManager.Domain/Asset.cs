namespace PhotoManager.Domain;

public class Asset
{
    // TODO: compose -> one readonly struct for File and another one for corruption ?
    public Guid FolderId { get; set; }
    public required Folder Folder { get; set; }
    public required string FileName { get; set; }
    public long FileSize { get; init; }
    public Pixel Pixel { get; init; }
    public Rotation ImageRotation { get; init; }
    public DateTime ThumbnailCreationDateTime { get; set; }
    public required string Hash { get; set; }
    public BitmapImage? ImageData { get; set; }
    public string FullPath => Path.Combine(Folder.Path, FileName);
    public DateTime FileCreationDateTime { get; set; }
    public DateTime FileModificationDateTime { get; set; }
    public bool IsAssetCorrupted { get; init; }
    public string? AssetCorruptedMessage { get; set; }
    public bool IsAssetRotated { get; init; }
    public string? AssetRotatedMessage { get; set; }
}
