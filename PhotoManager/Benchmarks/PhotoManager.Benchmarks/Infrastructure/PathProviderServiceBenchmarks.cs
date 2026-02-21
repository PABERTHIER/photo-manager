using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.Extensions.Configuration;
using PhotoManager.Infrastructure;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace PhotoManager.Benchmarks.Infrastructure;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class PathProviderServiceBenchmarks
{
    private string _testDirectory = null!;
    private PathProviderService _pathProviderService = null!;

    [GlobalSetup]
    public void Setup()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "PathProviderServiceBenchmark_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDirectory);

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
            ["appsettings:Paths:BackupPath"] = Path.Combine(_testDirectory, "Backup"),
            ["appsettings:Paths:ExemptedFolderPath"] = Path.Combine(_testDirectory, "Exempted"),
            ["appsettings:Paths:FirstFrameVideosFolderName"] = "VideoFirstFrame",
            ["appsettings:Project:Name"] = "PhotoManager",
            ["appsettings:Project:Owner"] = "Test",
            ["appsettings:Storage:BackupsToKeep"] = "2",
            ["appsettings:Storage:FoldersName:Blobs"] = "blobs",
            ["appsettings:Storage:FoldersName:Tables"] = "tables",
            ["appsettings:Storage:Separator"] = "|",
            ["appsettings:Storage:StorageVersion"] = "1.0",
            ["appsettings:Storage:Tables:AssetsTableName"] = "Assets",
            ["appsettings:Storage:Tables:FoldersTableName"] = "Folders",
            ["appsettings:Storage:Tables:RecentTargetPathsTableName"] = "RecentTargetPaths",
            ["appsettings:Storage:Tables:SyncAssetsDirectoriesDefinitionsTableName"] =
                "SyncAssetsDirectoriesDefinitions",
            ["appsettings:Storage:ThumbnailsDictionaryEntriesToKeep"] = "5"
        };

        IConfigurationRoot mockConfig = new ConfigurationBuilder().AddInMemoryCollection(configDict).Build();
        UserConfigurationService userConfigService = new(mockConfig);
        _pathProviderService = new PathProviderService(userConfigService);
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
    public string ResolveDataDirectory_New()
    {
        return _pathProviderService.ResolveDataDirectory();
    }

    [Benchmark]
    public string ResolveDataDirectory_AggressiveInlining()
    {
        return ResolveDataDirectoryAggressiveInlining("1.0");
    }

    [Benchmark]
    public string ResolveDataDirectory_Old()
    {
        return OldResolveDataDirectory("1.0");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string ResolveDataDirectoryAggressiveInlining(string storageVersion)
    {
        return Path.Combine(_testDirectory, "Backup", $"v{storageVersion}");
    }

    private string OldResolveDataDirectory(string storageVersion)
    {
        StringBuilder currentStorageVersion = new();
        currentStorageVersion.Append('v');
        currentStorageVersion.Append(storageVersion);
        return Path.Combine(_testDirectory, "Backup", currentStorageVersion.ToString());
    }
}
