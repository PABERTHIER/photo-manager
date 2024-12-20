namespace PhotoManager.Domain;

public class Asset
{
    public Guid FolderId { get; init; }
    public required Folder Folder { get; set; } // Not stored in the same table in DB
    public required string FileName { get; init; }
    public string FullPath => Path.Combine(Folder.Path, FileName);
    public FileProperties FileProperties { get; set; } // Not stored in DB
    public Pixel Pixel { get; init; }
    public Rotation ImageRotation { get; init; }
    public required string Hash { get; init; }
    public BitmapImage? ImageData { get; set; } // Not stored in DB
    public DateTime ThumbnailCreationDateTime { get; init; }
    public bool IsAssetCorrupted { get; init; }
    public string? AssetCorruptedMessage { get; init; }
    public bool IsAssetRotated { get; init; }
    public string? AssetRotatedMessage { get; init; }

    // Used for tests only, to make FolderId and Folder properties immutable
    public Asset WithFolder(Folder folder)
    {
        return new Asset
        {
            FolderId = folder.FolderId,
            Folder = folder,
            FileName = FileName,
            FileProperties = FileProperties,
            Pixel = Pixel,
            ImageRotation = ImageRotation,
            Hash = Hash,
            ImageData = ImageData,
            ThumbnailCreationDateTime = ThumbnailCreationDateTime,
            IsAssetCorrupted = IsAssetCorrupted,
            AssetCorruptedMessage = AssetCorruptedMessage,
            IsAssetRotated = IsAssetRotated,
            AssetRotatedMessage = AssetRotatedMessage
        };
    }

    // Used for tests only, to make Hash property immutable
    public Asset WithHash(string hash)
    {
        return new Asset
        {
            FolderId = FolderId,
            Folder = Folder,
            FileName = FileName,
            FileProperties = FileProperties,
            Pixel = Pixel,
            ImageRotation = ImageRotation,
            Hash = hash,
            ImageData = ImageData,
            ThumbnailCreationDateTime = ThumbnailCreationDateTime,
            IsAssetCorrupted = IsAssetCorrupted,
            AssetCorruptedMessage = AssetCorruptedMessage,
            IsAssetRotated = IsAssetRotated,
            AssetRotatedMessage = AssetRotatedMessage
        };
    }
}
