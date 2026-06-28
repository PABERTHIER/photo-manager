namespace PhotoManager.Domain;

public class Asset
{
    public required Guid FolderId { get; init; }
    public required Folder Folder { get; set; } // Not stored in the same table in DB
    public required string FileName { get; init; }
    public string FullPath => Path.Combine(Folder.Path, FileName);
    public FileProperties FileProperties { get; set; } // Not stored in DB
    public required Pixel Pixel { get; init; }
    public ImageRotation ImageRotation { get; init; }
    public required string Hash { get; init; }
    public IImageData? ImageData { get; set; } // Not stored in DB
    public DateTime ThumbnailCreationDateTime { get; init; }
    public Metadata Metadata { get; init; }
}
