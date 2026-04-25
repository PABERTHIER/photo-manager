namespace PhotoManager.Benchmarks.Domain;

// Context:
// AssetsComparator.GetImageAndVideoNamesList starts with zero-capacity List<string> instances
// (default List<T>() initialises with capacity 0 and doubles on each resize).
// For large file collections this causes several allocation+copy cycles.
// Pre-sizing imageNames to fileNames.Length avoids any resize (worst case: all files are images).
// videoNames is kept small by default since videos are typically a minority of a photo library.

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class AssetsComparatorGetImageAndVideoNamesBenchmarks
{
    private string[] _fileNames = null!;

    // Realistic distribution: 70% images, 15% videos, 15% unsupported
    [Params(100, 1000)] public int FileCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        string[] extensions =
        [
            ".jpg", ".jpg", ".jpg", ".jpg", ".png", ".png", ".gif", // ~54% images
            ".mp4", ".avi", ".mkv", // ~23% videos
            ".txt", ".exe", ".pdf" // ~23% unsupported
        ];

        _fileNames =
        [
            .. Enumerable.Range(1, FileCount).Select(i =>
                $"file_{i:D4}{extensions[i % extensions.Length]}")
        ];
    }

    // ── Benchmarks ────────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    public (string[], string[]) Original_DefaultCapacity()
    {
        List<string> imageNames = [];
        List<string> videoNames = [];

        foreach (string fileName in _fileNames)
        {
            if (ImageHelper.IsImageFile(fileName))
            {
                imageNames.Add(fileName);
            }
            else if (VideoHelper.IsVideoFile(fileName))
            {
                videoNames.Add(fileName);
            }
        }

        return ([.. imageNames], [.. videoNames]);
    }

    [Benchmark]
    public (string[], string[]) Optimized_PreSizedImageList()
    {
        // Pre-size imageNames to the total file count — avoids all resizes since images
        // are almost always the majority of a photo library folder.
        List<string> imageNames = new(_fileNames.Length);
        List<string> videoNames = [];

        foreach (string fileName in _fileNames)
        {
            if (ImageHelper.IsImageFile(fileName))
            {
                imageNames.Add(fileName);
            }
            else if (VideoHelper.IsVideoFile(fileName))
            {
                videoNames.Add(fileName);
            }
        }

        return ([.. imageNames], [.. videoNames]);
    }

    [Benchmark]
    public (string[], string[]) Optimized_PreSizedBothLists()
    {
        // Pre-size both lists to avoid resizing.
        // imageNames uses full length (safe upper bound); videoNames uses quarter estimate.
        List<string> imageNames = new(_fileNames.Length);
        List<string> videoNames = new(_fileNames.Length / 4);

        foreach (string fileName in _fileNames)
        {
            if (ImageHelper.IsImageFile(fileName))
            {
                imageNames.Add(fileName);
            }
            else if (VideoHelper.IsVideoFile(fileName))
            {
                videoNames.Add(fileName);
            }
        }

        return ([.. imageNames], [.. videoNames]);
    }
}
