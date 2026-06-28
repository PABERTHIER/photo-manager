namespace PhotoManager.Benchmarks.Infrastructure;

[MemoryDiagnoser]
[ShortRunJob]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class AssetRepositoryReadCatalogFilePropertiesBenchmarks
{
    private Asset[] _assets = null!;
    private string _directory = null!;
    private ParallelOptions _parallelOptions = null!;

    [Params(1_000, 5_000)]
    public int AssetCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _directory = Path.Combine(Path.GetTempPath(),
            "AssetRepositoryReadCatalogFileProperties_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);

        Folder folder = new() { Id = Guid.NewGuid(), Path = _directory };
        _assets = new Asset[AssetCount];

        for (int i = 0; i < AssetCount; i++)
        {
            string fileName = $"Image_{i:D6}.jpg";
            File.WriteAllText(Path.Combine(_directory, fileName), "benchmark");
            _assets[i] = AssetBenchmarkBuilder.Create()
                .WithFolder(folder)
                .WithFileName(fileName)
                .WithPixels(1920, 1080, 200, 150)
                .WithFileSize(0)
                .WithHash($"hash_{i:D6}")
                .WithThumbnailCreationDateTime(DateTime.UnixEpoch)
                .WithImageRotation(ImageRotation.Rotate0)
                .WithCorrupted(false, null)
                .WithRotated(false, null)
                .Build();
        }

        _parallelOptions = new()
        {
            MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 8)
        };
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, true);
        }
    }

    [Benchmark(Baseline = true)]
    public void SequentialFileStats()
    {
        for (int i = 0; i < _assets.Length; i++)
        {
            UpdateFileProperties(_assets[i]);
        }
    }

    [Benchmark]
    public void ParallelFileStats()
    {
        Parallel.For(0, _assets.Length, _parallelOptions, i => UpdateFileProperties(_assets[i]));
    }

    private static void UpdateFileProperties(Asset asset)
    {
        if (!File.Exists(asset.FullPath))
        {
            return;
        }

        FileInfo info = new(asset.FullPath);

        asset.FileProperties = new()
        {
            Size = info.Length,
            Creation = info.CreationTime,
            Modification = info.LastWriteTime
        };
    }
}
