namespace PhotoManager.Benchmarks.Domain;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class FolderThumbnailsFilenameBenchmarks
{
    private FolderOriginal _folderOriginal = null!;
    private FolderCached _folderCached = null!;

    [GlobalSetup]
    public void Setup()
    {
        Guid id = Guid.NewGuid();
        _folderOriginal = new() { Id = id, Path = @"C:\Users\Photos" };
        _folderCached = new() { Id = id, Path = @"C:\Users\Photos", ThumbnailsFilename = $"{id}.bin" };
    }

    // Simulates the realistic case: ThumbnailsFilename accessed multiple times per Folder
    // (5 calls in AssetRepository + 1 in CatalogAssetsService)

    [Benchmark(Baseline = true)]
    public string Original_6Calls()
    {
        _ = _folderOriginal.ThumbnailsFilename;
        _ = _folderOriginal.ThumbnailsFilename;
        _ = _folderOriginal.ThumbnailsFilename;
        _ = _folderOriginal.ThumbnailsFilename;
        _ = _folderOriginal.ThumbnailsFilename;

        return _folderOriginal.ThumbnailsFilename;
    }

    [Benchmark]
    public string Optimized_Cached_6Calls()
    {
        _ = _folderCached.ThumbnailsFilename;
        _ = _folderCached.ThumbnailsFilename;
        _ = _folderCached.ThumbnailsFilename;
        _ = _folderCached.ThumbnailsFilename;
        _ = _folderCached.ThumbnailsFilename;

        return _folderCached.ThumbnailsFilename;
    }

    [Benchmark]
    public string Original_SingleCall()
    {
        return _folderOriginal.ThumbnailsFilename;
    }

    [Benchmark]
    public string Optimized_Cached_SingleCall()
    {
        return _folderCached.ThumbnailsFilename;
    }
}

// Mirrors old Folder implementation
internal class FolderOriginal
{
    public required Guid Id { get; init; }
    public required string Path { get; init; }
    public string ThumbnailsFilename => Id + ".bin";
}

// Pre-computed variant: ThumbnailsFilename set once at construction, then just a property read
internal class FolderCached
{
    public required Guid Id { get; init; }
    public required string Path { get; init; }
    public required string ThumbnailsFilename { get; init; }
}
