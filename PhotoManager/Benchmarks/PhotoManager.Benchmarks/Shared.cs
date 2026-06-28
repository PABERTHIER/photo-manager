namespace PhotoManager.Benchmarks;

internal static class Shared
{
    private static readonly string[] JpgFileNames =
    [
        "Image 1.jpg",
        "Image 1_90_deg.jpg",
        "Image 1_180_deg.jpg",
        "Image 1_270_deg.jpg"
    ];

    private static string FindTestFilesDirectory()
    {
        string? directory = AppContext.BaseDirectory;

        while (directory != null)
        {
            string testFilesPath = Path.Combine(
                directory,
                "PhotoManager",
                "PhotoManager.Tests",
                "TestFiles");

            if (Directory.Exists(testFilesPath))
            {
                return testFilesPath;
            }

            directory = Path.GetDirectoryName(directory);
        }

        throw new DirectoryNotFoundException("Could not find TestFiles directory.");
    }

    internal static byte[][] LoadJpgImageBuffers()
    {
        string testFilesDir = FindTestFilesDirectory();

        return [.. JpgFileNames.Select(f => File.ReadAllBytes(Path.Combine(testFilesDir, f)))];
    }
}

internal sealed class AssetBenchmarkBuilder
{
    private Guid? _folderId;
    private Folder? _folder;
    private string? _fileName;
    private FileProperties? _fileProperties;
    private Dimensions? _assetPixels;
    private Dimensions? _thumbnailPixels;
    private ImageRotation? _imageRotation;
    private string? _hash;
    private DateTime? _thumbnailCreationDateTime;
    private Flag? _corrupted;
    private Flag? _rotated;

    public static AssetBenchmarkBuilder Create() => new();

    // Sets both Folder and FolderId (= folder.Id).
    // Call WithFolderId afterwards to override the id alone (used when the asset's FolderId intentionally differs from its Folder.Id).
    public AssetBenchmarkBuilder WithFolder(Folder folder)
    {
        _folder = folder;
        _folderId = folder.Id;

        return this;
    }

    public AssetBenchmarkBuilder WithFolderPath(string path, Guid id)
    {
        return WithFolder(new() { Id = id, Path = path });
    }

    public AssetBenchmarkBuilder WithFolderId(Guid folderId)
    {
        _folderId = folderId;

        return this;
    }

    public AssetBenchmarkBuilder WithFileName(string fileName)
    {
        _fileName = fileName;

        return this;
    }

    public AssetBenchmarkBuilder WithHash(string hash)
    {
        _hash = hash;

        return this;
    }

    public AssetBenchmarkBuilder WithAssetPixels(int width, int height)
    {
        _assetPixels = new() { Width = width, Height = height };

        return this;
    }

    public AssetBenchmarkBuilder WithThumbnailPixels(int width, int height)
    {
        _thumbnailPixels = new() { Width = width, Height = height };

        return this;
    }

    public AssetBenchmarkBuilder WithPixels(int assetWidth, int assetHeight, int thumbnailWidth, int thumbnailHeight)
    {
        return WithAssetPixels(assetWidth, assetHeight).WithThumbnailPixels(thumbnailWidth, thumbnailHeight);
    }

    public AssetBenchmarkBuilder WithImageRotation(ImageRotation rotation)
    {
        _imageRotation = rotation;

        return this;
    }

    public AssetBenchmarkBuilder WithFileSize(long size)
    {
        _fileProperties = new() { Size = size };

        return this;
    }

    public AssetBenchmarkBuilder WithFileProperties(long size, DateTime creation, DateTime modification)
    {
        _fileProperties = new() { Size = size, Creation = creation, Modification = modification };

        return this;
    }

    public AssetBenchmarkBuilder WithThumbnailCreationDateTime(DateTime thumbnailCreationDateTime)
    {
        _thumbnailCreationDateTime = thumbnailCreationDateTime;

        return this;
    }

    public AssetBenchmarkBuilder WithCorrupted(bool isTrue, string? message)
    {
        _corrupted = new() { IsTrue = isTrue, Message = message };

        return this;
    }

    public AssetBenchmarkBuilder WithRotated(bool isTrue, string? message)
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
            ThumbnailCreationDateTime = _thumbnailCreationDateTime!.Value,
            Metadata = new() { Corrupted = _corrupted!.Value, Rotated = _rotated!.Value }
        };
    }
}
