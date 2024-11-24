namespace PhotoManager.Domain;

public class Asset
{
    public Guid FolderId { get; set; }
    public required Folder Folder { get; set; }
    public required string FileName { get; set; }
    public string FullPath => Path.Combine(Folder.Path, FileName);
    public long FileSize { get; init; }
    public FileDateTime FileDateTime { get; set; }
    public Pixel Pixel { get; init; }
    public Rotation ImageRotation { get; init; }
    public required string Hash { get; set; }
    public BitmapImage? ImageData { get; set; }
    public DateTime ThumbnailCreationDateTime { get; init; }
    public bool IsAssetCorrupted { get; init; }
    public string? AssetCorruptedMessage { get; set; }
    public bool IsAssetRotated { get; init; }
    public string? AssetRotatedMessage { get; set; }
}
