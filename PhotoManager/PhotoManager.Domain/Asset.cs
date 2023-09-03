namespace PhotoManager.Domain;

public class Asset
{
    // TODO: use intead Folder.FolderId ? -> if done, update tests
    public string FolderId { get; set; } // Why not a Guid? ?
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

    // Keep this code for UT only, whereas, one UT will fail... -> investigate which one is it
    public override bool Equals(object? obj)
    {
        return obj is Asset asset && asset.FolderId == FolderId && asset.FileName == FileName;
    }

    // Only for UT
    public override int GetHashCode()
    {
        // TODO: if FolderId has been passed into Guid?, it has to be .ToString() before the GetHashCode() calling
        return (!string.IsNullOrEmpty(FolderId) ? FolderId.GetHashCode() : base.GetHashCode()) + (!string.IsNullOrEmpty(FileName) ? FileName.GetHashCode() : base.GetHashCode());
    }

    // Only for UT
    public override string ToString()
    {
        return FileName;
    }
}
