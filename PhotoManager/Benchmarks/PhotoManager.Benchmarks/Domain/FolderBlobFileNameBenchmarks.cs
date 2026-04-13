namespace PhotoManager.Benchmarks.Domain;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class FolderBlobFileNameBenchmarks
{
    private FolderOriginal _folderOriginal = null!;
    private FolderCached _folderCached = null!;

    [GlobalSetup]
    public void Setup()
    {
        Guid id = Guid.NewGuid();
        _folderOriginal = new() { Id = id, Path = @"C:\Users\Photos" };
        _folderCached = new() { Id = id, Path = @"C:\Users\Photos", BlobFileName = $"{id}.bin" };
    }

    // Simulates the realistic case: BlobFileName accessed multiple times per Folder
    // (5 calls in AssetRepository + 1 in CatalogAssetsService)

    [Benchmark(Baseline = true)]
    public string Original_6Calls()
    {
        _ = _folderOriginal.BlobFileName;
        _ = _folderOriginal.BlobFileName;
        _ = _folderOriginal.BlobFileName;
        _ = _folderOriginal.BlobFileName;
        _ = _folderOriginal.BlobFileName;

        return _folderOriginal.BlobFileName;
    }

    [Benchmark]
    public string Optimized_Cached_6Calls()
    {
        _ = _folderCached.BlobFileName;
        _ = _folderCached.BlobFileName;
        _ = _folderCached.BlobFileName;
        _ = _folderCached.BlobFileName;
        _ = _folderCached.BlobFileName;

        return _folderCached.BlobFileName;
    }

    [Benchmark]
    public string Original_SingleCall()
    {
        return _folderOriginal.BlobFileName;
    }

    [Benchmark]
    public string Optimized_Cached_SingleCall()
    {
        return _folderCached.BlobFileName;
    }
}

// Mirrors old Folder implementation
internal class FolderOriginal
{
    public required Guid Id { get; init; }
    public required string Path { get; init; }
    public string BlobFileName => Id + ".bin";
}

// Pre-computed variant: BlobFileName set once at construction, then just a property read
internal class FolderCached
{
    public required Guid Id { get; init; }
    public required string Path { get; init; }
    public required string BlobFileName { get; init; }
}
