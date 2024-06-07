namespace PhotoManager.Domain;

public class Asset
{
    // TODO: use instead Folder.FolderId ? -> if done, update tests
    // TODO: property required
    public Guid FolderId { get; set; }
    public Folder Folder { get; set; }
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public int PixelWidth { get; set; }
    public int PixelHeight { get; set; }
    public int ThumbnailPixelWidth { get; set; }
    public int ThumbnailPixelHeight { get; set; }
    public Rotation ImageRotation { get; set; }
    public DateTime ThumbnailCreationDateTime { get; set; }
    public string Hash { get; set; }
    public BitmapImage? ImageData { get; set; }
    public string FullPath => Folder != null ? Path.Combine(Folder.Path, FileName) : FileName;
    public DateTime FileCreationDateTime { get; set; }
    public DateTime FileModificationDateTime { get; set; }
    public bool IsAssetCorrupted { get; set; }
    public string? AssetCorruptedMessage { get; set; }
    public bool IsAssetRotated { get; set; }
    public string? AssetRotatedMessage { get; set; }
}
