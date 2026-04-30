namespace PhotoManager.Benchmarks.Infrastructure;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class AssetRepositoryLookupBenchmarks
{
    private List<Folder> _folders = null!;
    private List<Asset> _assets = null!;
    private Dictionary<string, Folder> _foldersByPath = null!;
    private Dictionary<Guid, Folder> _foldersById = null!;
    private Dictionary<Guid, Dictionary<string, Asset>> _assetsByFolderId = null!;

    private string _targetPath = null!;
    private Guid _targetFolderId;
    private string _targetFileName = null!;

    [Params(100, 1_000, 10_000)] public int FolderCount { get; set; }

    [Params(10)] public int AssetsPerFolder { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _folders = new(FolderCount);
        _foldersByPath = new(FolderCount);
        _foldersById = new(FolderCount);
        _assets = new(FolderCount * AssetsPerFolder);
        _assetsByFolderId = new(FolderCount);

        for (int i = 0; i < FolderCount; i++)
        {
            Folder folder = new() { Id = Guid.NewGuid(), Path = $@"C:\Photos\Folder{i}" };
            _folders.Add(folder);
            _foldersByPath[folder.Path] = folder;
            _foldersById[folder.Id] = folder;

            Dictionary<string, Asset> folderAssets = new(AssetsPerFolder);
            _assetsByFolderId[folder.Id] = folderAssets;

            for (int j = 0; j < AssetsPerFolder; j++)
            {
                Asset asset = new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = $"Image_{j}.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1920, Height = 1080 },
                        Thumbnail = new() { Width = 200, Height = 150 }
                    },
                    Hash = "abc123"
                };

                _assets.Add(asset);
                folderAssets[asset.FileName] = asset;
            }
        }

        int targetIndex = FolderCount / 2;
        _targetPath = _folders[targetIndex].Path;
        _targetFolderId = _folders[targetIndex].Id;
        _targetFileName = "Image_5.jpg";
    }

    #region GetFolderByPath

    [Benchmark(Baseline = true)]
    public Folder? GetFolderByPath_List()
    {
        return _folders.FirstOrDefault(f => f.Path == _targetPath);
    }

    [Benchmark]
    public Folder? GetFolderByPath_Dictionary()
    {
        return _foldersByPath.GetValueOrDefault(_targetPath);
    }

    #endregion

    #region FolderExists

    [Benchmark]
    public bool FolderExists_List()
    {
        return _folders.Any(f => f.Path == _targetPath);
    }

    [Benchmark]
    public bool FolderExists_Dictionary()
    {
        return _foldersByPath.ContainsKey(_targetPath);
    }

    #endregion

    #region GetFolderById

    [Benchmark]
    public Folder? GetFolderById_List()
    {
        return _folders.FirstOrDefault(f => f.Id == _targetFolderId);
    }

    [Benchmark]
    public Folder? GetFolderById_Dictionary()
    {
        return _foldersById.GetValueOrDefault(_targetFolderId);
    }

    #endregion

    #region GetAssetByFolderIdAndFileName

    [Benchmark]
    public Asset? GetAssetByFolderIdAndFileName_List()
    {
        return _assets.FirstOrDefault(a => a.FolderId == _targetFolderId && a.FileName == _targetFileName);
    }

    [Benchmark]
    public Asset? GetAssetByFolderIdAndFileName_Dictionary()
    {
        if (_assetsByFolderId.TryGetValue(
                _targetFolderId, out Dictionary<string, Asset>? folderAssets))
        {
            return folderAssets.GetValueOrDefault(_targetFileName);
        }

        return null;
    }

    #endregion

    #region GetAssetsByFolderId

    [Benchmark]
    public List<Asset> GetAssetsByFolderId_List()
    {
        return [.. _assets.Where(a => a.FolderId == _targetFolderId)];
    }

    [Benchmark]
    public List<Asset> GetAssetsByFolderId_Dictionary()
    {
        return _assetsByFolderId.TryGetValue(
            _targetFolderId, out Dictionary<string, Asset>? folderAssets)
            ? [.. folderAssets.Values]
            : [];
    }

    #endregion
}
