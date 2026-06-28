namespace PhotoManager.Benchmarks.Infrastructure;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[ShortRunJob]
public class ImageMetadataServiceUpdateAssetsFilePropertiesBenchmarks
{
    private Asset[] _assets = null!;
    private Folder _folder = null!;
    private string _testDirectory = null!;

    [Params(100, 1_000)]
    public int AssetCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _testDirectory = Path.Combine(
            Path.GetTempPath(), $"{nameof(ImageMetadataServiceUpdateAssetsFilePropertiesBenchmarks)}_{AssetCount}");
        Directory.CreateDirectory(_testDirectory);
        _folder = new() { Id = Guid.NewGuid(), Path = _testDirectory };
        _assets = new Asset[AssetCount];

        for (int i = 0; i < AssetCount; i++)
        {
            string fileName = $"Image_{i}.jpg";
            string filePath = Path.Combine(_testDirectory, fileName);
            File.WriteAllBytes(filePath, [1, 2, 3, 4]);
            _assets[i] = CreateAsset(_folder, fileName);
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Benchmark(Baseline = true)]
    public int Current_GetFileNamesThenStatEachAsset()
    {
        string[] fileNames = Directory.GetFiles(_testDirectory).Select(Path.GetFileName).ToArray()!;
        UpdateAssetsFilePropertiesFromFileInfo(_assets);
        return fileNames.Length;
    }

    [Benchmark]
    public int Optimized_EnumerateFileInfosOnce()
    {
        FileInfo[] fileInfos = [.. new DirectoryInfo(_testDirectory).EnumerateFiles()];
        Dictionary<string, FileProperties> filePropertiesByName = CreateFilePropertiesByName(fileInfos);
        UpdateAssetsFilePropertiesFromDictionary(_assets, filePropertiesByName);
        return fileInfos.Length;
    }

    private static Dictionary<string, FileProperties> CreateFilePropertiesByName(FileInfo[] fileInfos)
    {
        Dictionary<string, FileProperties> filePropertiesByName = new(fileInfos.Length, StringComparer.Ordinal);

        for (int i = 0; i < fileInfos.Length; i++)
        {
            FileInfo fileInfo = fileInfos[i];
            filePropertiesByName[fileInfo.Name] = new()
            {
                Size = fileInfo.Length,
                Creation = fileInfo.CreationTime,
                Modification = fileInfo.LastWriteTime
            };
        }

        return filePropertiesByName;
    }

    private static void UpdateAssetsFilePropertiesFromFileInfo(Asset[] assets)
    {
        for (int i = 0; i < assets.Length; i++)
        {
            Asset asset = assets[i];

            if (!File.Exists(asset.FullPath))
            {
                continue;
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

    private static void UpdateAssetsFilePropertiesFromDictionary(Asset[] assets,
        IReadOnlyDictionary<string, FileProperties> filePropertiesByName)
    {
        for (int i = 0; i < assets.Length; i++)
        {
            Asset asset = assets[i];

            if (filePropertiesByName.TryGetValue(asset.FileName, out FileProperties fileProperties))
            {
                asset.FileProperties = fileProperties;
            }
        }
    }

    private static Asset CreateAsset(Folder folder, string fileName) =>
        AssetBenchmarkBuilder.Create()
            .WithFolder(folder)
            .WithFileName(fileName)
            .WithPixels(100, 100, 50, 50)
            .WithFileSize(0)
            .WithHash(fileName)
            .WithThumbnailCreationDateTime(DateTime.UnixEpoch)
            .WithImageRotation(ImageRotation.Rotate0)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
}
