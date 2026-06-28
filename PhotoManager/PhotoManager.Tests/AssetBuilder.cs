namespace PhotoManager.Tests;

public sealed class AssetBuilder
{
    private Guid? _folderId;
    private Folder? _folder;
    private string? _fileName;
    private FileProperties? _fileProperties;
    private Dimensions? _assetPixels;
    private Dimensions? _thumbnailPixels;
    private ImageRotation? _imageRotation;
    private string? _hash;
    private IImageData? _imageData;
    private DateTime? _thumbnailCreationDateTime;
    private Flag? _corrupted;
    private Flag? _rotated;

    public static AssetBuilder Create() => new();

    // Sets both Folder and FolderId (= folder.Id).
    // Call WithFolderId afterwards to override the id alone (used by the few tests where the asset's FolderId intentionally differs from its Folder.Id).
    public AssetBuilder WithFolder(Folder? folder)
    {
        _folder = folder;
        _folderId = folder?.Id;

        return this;
    }

    public AssetBuilder WithFolderPath(string path, Guid id)
    {
        return WithFolder(new() { Id = id, Path = path });
    }

    public AssetBuilder WithFolderId(Guid folderId)
    {
        _folderId = folderId;

        return this;
    }

    public AssetBuilder WithFileName(string fileName)
    {
        _fileName = fileName;

        return this;
    }

    public AssetBuilder WithHash(string hash)
    {
        _hash = hash;

        return this;
    }

    public AssetBuilder WithAssetPixels(int width, int height)
    {
        _assetPixels = new() { Width = width, Height = height };

        return this;
    }

    public AssetBuilder WithThumbnailPixels(int width, int height)
    {
        _thumbnailPixels = new() { Width = width, Height = height };

        return this;
    }

    public AssetBuilder WithPixels(int assetWidth, int assetHeight, int thumbnailWidth, int thumbnailHeight)
    {
        return WithAssetPixels(assetWidth, assetHeight).WithThumbnailPixels(thumbnailWidth, thumbnailHeight);
    }

    public AssetBuilder WithRotation(ImageRotation rotation)
    {
        _imageRotation = rotation;

        return this;
    }

    public AssetBuilder WithFileSize(long size)
    {
        _fileProperties = new() { Size = size };

        return this;
    }

    public AssetBuilder WithFileProperties(long size, DateTime creation, DateTime modification)
    {
        _fileProperties = new() { Size = size, Creation = creation, Modification = modification };

        return this;
    }

    public AssetBuilder WithThumbnailCreationDateTime(DateTime thumbnailCreationDateTime)
    {
        _thumbnailCreationDateTime = thumbnailCreationDateTime;

        return this;
    }

    public AssetBuilder WithImageData(IImageData? imageData)
    {
        _imageData = imageData;

        return this;
    }

    public AssetBuilder WithCorrupted(bool isTrue, string? message)
    {
        _corrupted = new() { IsTrue = isTrue, Message = message };

        return this;
    }

    public AssetBuilder WithRotated(bool isTrue, string? message)
    {
        _rotated = new() { IsTrue = isTrue, Message = message };

        return this;
    }

    public Asset Build()
    {
        return new()
        {
            FolderId = _folderId!.Value,
            Folder = _folder!,
            FileName = _fileName!,
            FileProperties = _fileProperties!.Value,
            Pixel = new() { Asset = _assetPixels!.Value, Thumbnail = _thumbnailPixels!.Value },
            ImageRotation = _imageRotation!.Value,
            Hash = _hash!,
            ImageData = _imageData,
            ThumbnailCreationDateTime = _thumbnailCreationDateTime!.Value,
            Metadata = new() { Corrupted = _corrupted!.Value, Rotated = _rotated!.Value }
        };
    }
}
