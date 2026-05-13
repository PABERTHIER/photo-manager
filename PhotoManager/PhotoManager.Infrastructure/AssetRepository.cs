using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PhotoManager.Persistence;
using PhotoManager.Persistence.Sqlite;
using System.Reactive;

namespace PhotoManager.Infrastructure;

/// <summary>
/// Thin forwarder that delegates all <see cref="IAssetRepository"/> calls to
/// <see cref="AssetRepositoryAdapter"/>. Exists as a migration shim so that
/// existing DI registrations continue to work unchanged.
/// Phase 2 will remove this class and use OptimizedAssetRepository.
/// </summary>
public class AssetRepository : IAssetRepository, IDisposable
{
    private readonly AssetRepositoryAdapter _assetRepositoryAdapter;
    private readonly OptimizedAssetRepository? _optimizedAssetRepository;
    private readonly IPersistenceContext? _persistenceContext;

    /// <summary>Primary constructor used by DI.</summary>
    public AssetRepository(AssetRepositoryAdapter adapter)
    {
        _assetRepositoryAdapter = adapter;
    }

    /// <summary>
    /// Convenience constructor that creates the full SQLite chain internally.
    /// Used by integration tests that do not need thumbnail inspection.
    /// </summary>
    public AssetRepository(
        IPathProviderService pathProviderService,
        IImageProcessingService imageProcessingService,
        IImageMetadataService imageMetadataService,
        IUserConfigurationService userConfigurationService,
        ILogger<AssetRepository> logger)
    {
        _ = logger;

        SqlitePersistenceContext sqlitePersistenceContext = new(NullLogger<SqlitePersistenceContext>.Instance);
        OptimizedAssetRepository optimizedAssetRepository = new(
            sqlitePersistenceContext,
            pathProviderService,
            imageProcessingService,
            imageMetadataService,
            userConfigurationService,
            NullLogger<OptimizedAssetRepository>.Instance);
        AssetRepositoryAdapter assetRepositoryAdapter = new(optimizedAssetRepository);

        _assetRepositoryAdapter = assetRepositoryAdapter;
        _optimizedAssetRepository = optimizedAssetRepository;
        _persistenceContext = sqlitePersistenceContext;
    }

    public IObservable<Unit> AssetsUpdated => _assetRepositoryAdapter.AssetsUpdated;

    public Asset[] GetAssetsByPath(string directory)
    {
        return _assetRepositoryAdapter.GetAssetsByPath(directory);
    }

    public void AddAsset(Asset asset, byte[] thumbnailData)
    {
        _assetRepositoryAdapter.AddAsset(asset, thumbnailData);
    }

    public Folder AddFolder(string path)
    {
        return _assetRepositoryAdapter.AddFolder(path);
    }

    public bool FolderExists(string path)
    {
        return _assetRepositoryAdapter.FolderExists(path);
    }

    public Folder[] GetFolders()
    {
        return _assetRepositoryAdapter.GetFolders();
    }

    public HashSet<string> GetFoldersPath()
    {
        return _assetRepositoryAdapter.GetFoldersPath();
    }

    public Folder[] GetSubFolders(Folder parentFolder)
    {
        return _assetRepositoryAdapter.GetSubFolders(parentFolder);
    }

    public Folder? GetFolderByPath(string path)
    {
        return _assetRepositoryAdapter.GetFolderByPath(path);
    }

    public bool BackupExists()
    {
        return _assetRepositoryAdapter.BackupExists();
    }

    public void WriteBackup()
    {
        _assetRepositoryAdapter.WriteBackup();
    }

    public List<Asset> GetCataloguedAssets()
    {
        return _assetRepositoryAdapter.GetCataloguedAssets();
    }

    public List<Asset> GetCataloguedAssetsByPath(string directory)
    {
        return _assetRepositoryAdapter.GetCataloguedAssetsByPath(directory);
    }

    public bool IsAssetCatalogued(string directoryName, string fileName)
    {
        return _assetRepositoryAdapter.IsAssetCatalogued(directoryName, fileName);
    }

    public Asset? DeleteAsset(string directory, string deletedFileName)
    {
        return _assetRepositoryAdapter.DeleteAsset(directory, deletedFileName);
    }

    public void DeleteFolder(Folder folder)
    {
        _assetRepositoryAdapter.DeleteFolder(folder);
    }

    public BitmapImage? LoadThumbnail(string directoryName, string fileName, int width, int height)
    {
        return _assetRepositoryAdapter.LoadThumbnail(directoryName, fileName, width, height);
    }

    public SyncAssetsConfiguration GetSyncAssetsConfiguration()
    {
        return _assetRepositoryAdapter.GetSyncAssetsConfiguration();
    }

    public void SaveSyncAssetsConfiguration(SyncAssetsConfiguration syncAssetsConfiguration)
    {
        _assetRepositoryAdapter.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
    }

    public List<string> GetRecentTargetPaths()
    {
        return _assetRepositoryAdapter.GetRecentTargetPaths();
    }

    public void SaveRecentTargetPaths(List<string> recentTargetPaths)
    {
        _assetRepositoryAdapter.SaveRecentTargetPaths(recentTargetPaths);
    }

    public void UpdateTargetPathToRecent(Folder destinationFolder)
    {
        _assetRepositoryAdapter.UpdateTargetPathToRecent(destinationFolder);
    }

    public int GetAssetsCounter()
    {
        return _assetRepositoryAdapter.GetAssetsCounter();
    }

    public void Dispose()
    {
        _optimizedAssetRepository?.Dispose();
        (_persistenceContext as IDisposable)?.Dispose();
        GC.SuppressFinalize(this);
    }
}
