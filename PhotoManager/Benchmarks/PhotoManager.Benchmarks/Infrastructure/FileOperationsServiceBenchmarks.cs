namespace PhotoManager.Benchmarks.Infrastructure;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class FileOperationsServiceBenchmarks
{
    private string _testDirectory = null!;
    private string _fileNamesDirectory = null!;
    private FileOperationsService _fileOperationsService = null!;

    [GlobalSetup]
    public void Setup()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(),
            "FileOperationsServiceBenchmark_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDirectory);
        _fileNamesDirectory = Path.Combine(_testDirectory, "FileNames");
        Directory.CreateDirectory(_fileNamesDirectory);

        // Create nested directory structure for recursive tests
        // Root: 5 subDirs, each with 3 files
        // Level 1: Each subDir has 3 subDirs with 2 files each
        // Total: ~45 directories, ~75 files
        for (int i = 0; i < 5; i++)
        {
            string subDir1 = Path.Combine(_testDirectory, $"SubDir{i}");
            Directory.CreateDirectory(subDir1);

            // Create files in level 1
            for (int f = 0; f < 3; f++)
            {
                File.WriteAllText(Path.Combine(subDir1, $"file{f}.txt"), "test");
            }

            // Create level 2 subdirectories
            for (int j = 0; j < 3; j++)
            {
                string subDir2 = Path.Combine(subDir1, $"SubDir{i}_{j}");
                Directory.CreateDirectory(subDir2);

                for (int f = 0; f < 2; f++)
                {
                    File.WriteAllText(Path.Combine(subDir2, $"file{f}.txt"), "test");
                }
            }
        }

        for (int i = 0; i < 1_000; i++)
        {
            File.WriteAllText(Path.Combine(_fileNamesDirectory, $"file_{i:D4}.txt"), "test");
        }

        Dictionary<string, string?> configDict = new()
        {
            ["appsettings:Asset:AnalyseVideos"] = "false",
            ["appsettings:Asset:CorruptedMessage"] = "The asset is corrupted",
            ["appsettings:Asset:RotatedMessage"] = "The asset has been rotated",
            ["appsettings:Asset:CatalogBatchSize"] = "100",
            ["appsettings:Asset:CatalogCooldownMinutes"] = "5",
            ["appsettings:Asset:CorruptedImageOrientation"] = "10000",
            ["appsettings:Asset:DefaultExifOrientation"] = "1",
            ["appsettings:Asset:DetectThumbnails"] = "false",
            ["appsettings:Asset:SyncAssetsEveryXMinutes"] = "false",
            ["appsettings:Asset:ThumbnailMaxHeight"] = "150",
            ["appsettings:Asset:ThumbnailMaxWidth"] = "200",
            ["appsettings:Hash:PHashThreshold"] = "10",
            ["appsettings:Hash:UsingDHash"] = "false",
            ["appsettings:Hash:UsingMD5Hash"] = "false",
            ["appsettings:Hash:UsingPHash"] = "false",
            ["appsettings:Paths:AssetsDirectory"] = _testDirectory,
            ["appsettings:Paths:ExemptedFolderPath"] = Path.Combine(_testDirectory, "Exempted"),
            ["appsettings:Paths:FirstFrameVideosFolderName"] = "VideoFirstFrame",
            ["appsettings:Project:Name"] = "PhotoManager",
            ["appsettings:Project:Owner"] = "Test",
            ["appsettings:Storage:BackupsToKeep"] = "2",
            ["appsettings:Storage:ThumbnailsDictionaryEntriesToKeep"] = "5"
        };

        IConfigurationRoot mockConfig = new ConfigurationBuilder().AddInMemoryCollection(configDict).Build();
        UserConfigurationService userConfigService = new(mockConfig);
        _fileOperationsService = new(userConfigService, NullLogger<FileOperationsService>.Instance);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Benchmark]
    public DirectoryInfo[] GetRecursiveSubDirectories_New()
    {
        return _fileOperationsService.GetRecursiveSubDirectories(_testDirectory);
    }

    [Benchmark]
    public List<DirectoryInfo> GetRecursiveSubDirectories_Old()
    {
        return GetRecursiveSubDirectories(_testDirectory);
    }

    private List<DirectoryInfo> GetRecursiveSubDirectories(string directoryPath)
    {
        List<DirectoryInfo> result = [];
        GetRecursiveSubDirectories(directoryPath, result);
        return result;
    }

    private void GetRecursiveSubDirectories(string directoryPath, List<DirectoryInfo> result)
    {
        List<DirectoryInfo> subDirectories = GetSubDirectories(directoryPath);
        result.AddRange(subDirectories);

        foreach (DirectoryInfo dir in subDirectories)
        {
            GetRecursiveSubDirectories(dir.FullName, result);
        }
    }

    private List<DirectoryInfo> GetSubDirectories(string directoryPath)
    {
        return [.. new DirectoryInfo(directoryPath).EnumerateDirectories()];
    }

    [Benchmark]
    public string[] GetFileNames_Current()
    {
        return _fileOperationsService.GetFileNames(_fileNamesDirectory);
    }

    [Benchmark]
    public string[] GetFileNames_InPlaceFromPaths()
    {
        string[] filePaths = Directory.GetFiles(_fileNamesDirectory);

        for (int i = 0; i < filePaths.Length; i++)
        {
            filePaths[i] = Path.GetFileName(filePaths[i]);
        }

        return filePaths;
    }
}
