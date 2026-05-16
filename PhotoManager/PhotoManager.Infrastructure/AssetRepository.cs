using Microsoft.Extensions.Logging;
using PhotoManager.Persistence;
using PhotoManager.Persistence.Cache;
using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PhotoManager.Infrastructure;

public class AssetRepository : IAssetRepository, IDisposable
{
    private const int RECENT_TARGET_PATHS_MAX_COUNT = 20;

    private readonly string _dataDirectory;
    private int _totalAssetCount;
    private string[] _recentTargetPaths = [];

    private readonly IImageProcessingService _imageProcessingService;
    private readonly IImageMetadataService _imageMetadataService;
    private readonly IUserConfigurationService _userConfigurationService;
    private readonly IPersistenceContext _persistenceContext;
    private readonly ILogger<AssetRepository> _logger;

    private readonly ConcurrentDictionary<string, Folder> _foldersByPath = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<Guid, Folder> _foldersById = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, Asset>> _assetsByFolderId = new();

    private readonly LruCache<Guid, ConcurrentDictionary<string, byte[]>> _thumbnailCache;

    private SyncAssetsConfiguration _syncAssetsConfiguration = new();

    private readonly Lock _folderCreationLock = new();
    private readonly Lock _recentPathsLock = new();
    private readonly Lock _backupLock = new();

    private readonly Subject<Unit> _assetsUpdatedSubject = new();
    private bool _disposed;

    public IObservable<Unit> AssetsUpdated => _assetsUpdatedSubject.AsObservable();

    /// <summary>
    /// Convenience constructor that creates the full SQLite chain internally.
    /// Used by integration tests that do not need thumbnail inspection.
    /// </summary>
    public AssetRepository(
        IPathProviderService pathProviderService,
        IImageProcessingService imageProcessingService,
        IImageMetadataService imageMetadataService,
        IUserConfigurationService userConfigurationService,
        IPersistenceContext persistenceContext,
        ILogger<AssetRepository> logger)
    {
        _imageProcessingService = imageProcessingService;
        _imageMetadataService = imageMetadataService;
        _userConfigurationService = userConfigurationService;
        _persistenceContext = persistenceContext;
        _logger = logger;

        _dataDirectory = pathProviderService.ResolveDataDirectory();

        ushort cacheCapacity = userConfigurationService.StorageSettings.ThumbnailsDictionaryEntriesToKeep;

        if (cacheCapacity == 0)
        {
            cacheCapacity = 1;
        }

        _thumbnailCache = new LruCache<Guid, ConcurrentDictionary<string, byte[]>>(cacheCapacity);

        Initialize();
    }

    // -------------------------------------------------------------- Folders

    public Folder AddFolder(string path)
    {
        ThrowIfDisposed();

        if (_foldersByPath.TryGetValue(path, out Folder? existingFolder))
        {
            return existingFolder;
        }

        lock (_folderCreationLock)
        {
            if (_foldersByPath.TryGetValue(path, out existingFolder))
            {
                return existingFolder;
            }

            Folder folder = _persistenceContext.Folders.Insert(path);

            _foldersByPath[folder.Path] = folder;
            _foldersById[folder.Id] = folder;

            return folder;
        }
    }

    public bool FolderExists(string path)
    {
        return _foldersByPath.ContainsKey(path);
    }

    public Folder[] GetFolders()
    {
        return [.. _foldersById.Values];
    }

    public HashSet<string> GetFoldersPath()
    {
        string[] paths = [.. _foldersByPath.Keys];
        HashSet<string> foldersSet = new(paths.Length, StringComparer.Ordinal);

        foreach (string path in paths)
        {
            foldersSet.Add(path);
        }

        return foldersSet;
    }

    public Folder[] GetSubFolders(Folder parentFolder)
    {
        Folder[] folders = [.. _foldersById.Values];
        List<Folder> result = [];

        for (int i = 0; i < folders.Length; i++)
        {
            if (parentFolder.IsParentOf(folders[i]))
            {
                result.Add(folders[i]);
            }
        }

        return [.. result];
    }

    public Folder? GetFolderByPath(string path)
    {
        _foldersByPath.TryGetValue(path, out Folder? folder);
        return folder;
    }

    public void DeleteFolder(Folder folder)
    {
        try
        {
            ThrowIfDisposed();

            _thumbnailCache.Remove(folder.Id);

            if (_assetsByFolderId.TryRemove(folder.Id, out ConcurrentDictionary<string, Asset>? folderAssets))
            {
                Interlocked.Add(ref _totalAssetCount, -folderAssets.Count);
            }

            _persistenceContext.Assets.DeleteByFolderId(folder.Id);
            _persistenceContext.Thumbnails.DeleteByFolderId(folder.Id);
            _persistenceContext.Folders.Delete(folder.Id);

            if (_foldersByPath.TryGetValue(folder.Path, out Folder? folderExisting) && folderExisting.Id == folder.Id)
            {
                _foldersByPath.TryRemove(folder.Path, out _);
            }

            _foldersById.TryRemove(folder.Id, out _);
        }
        catch (Exception ex)
        {
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            _logger.LogError(ex, "Error deleting folder: {FolderPath}", folder?.Path);
            throw;
        }
    }

    // -------------------------------------------------------------- Assets

    public Asset[] GetAssetsByPath(string directoryPath)
    {
        try
        {
            Folder? folder = GetFolderByPath(directoryPath);

            if (folder == null)
            {
                return [];
            }

            Asset[] assets = GetAssetsByFolderId(folder.Id);

            try
            {
                LoadOrFetchThumbnails(folder, assets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading thumbnails for {Path}", directoryPath);
            }

            return FilterAssetsWithImageData(assets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assets for {Path}", directoryPath);
            return [];
        }
    }

    public bool AddAsset(Asset asset, byte[] thumbnailData)
    {
        ThrowIfDisposed();

        try
        {
            if (string.IsNullOrWhiteSpace(asset.Folder.Path))
            {
                _logger.LogError(
                    "The asset could not be added, folder path is null or empty, asset.FileName: {FileName}",
                    asset.FileName);
                return false;
            }

            AddAssetCore(asset, thumbnailData);

            return true;
        }
        catch (Exception ex)
        {
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            _logger.LogError(ex, "Error adding asset: {FileName}, FolderId: {FolderId}",
                asset?.FileName, asset?.FolderId);
            throw;
        }
    }

    public Asset? DeleteAsset(string directory, string deletedFileName)
    {
        Asset? deletedAsset;

        try
        {
            deletedAsset = RemoveAsset(directory, deletedFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting asset: {Directory}, {FileName}", directory, deletedFileName);
            throw;
        }

        if (deletedAsset != null)
        {
            _assetsUpdatedSubject.OnNext(Unit.Default);
        }

        return deletedAsset;
    }

    public Asset[] GetCataloguedAssets()
    {
        Asset[] assets = [.. _assetsByFolderId.Values.SelectMany(static inner => inner.Values)];

        // TODO: Why do we need this sort ?
        Array.Sort(assets, static (a, b) =>
        {
            int compare = string.Compare(a.FileName, b.FileName, StringComparison.Ordinal);

            return compare != 0 ? compare : string.Compare(a.Folder.Path, b.Folder.Path, StringComparison.Ordinal);
        });

        return assets;
    }

    public Asset[] GetCataloguedAssetsByPath(string directory)
    {
        Folder? folder = GetFolderByPath(directory);

        if (folder == null)
        {
            return [];
        }

        if (!_assetsByFolderId.TryGetValue(folder.Id, out ConcurrentDictionary<string, Asset>? folderAssets))
        {
            return [];
        }

        return [.. folderAssets.Values];
    }

    public bool IsAssetCatalogued(string directoryName, string fileName)
    {
        Folder? folder = GetFolderByPath(directoryName);

        if (folder == null)
        {
            return false;
        }

        return _assetsByFolderId.TryGetValue(folder.Id, out ConcurrentDictionary<string, Asset>? folderAssets)
               && folderAssets.ContainsKey(fileName);
    }

    public int GetAssetsCounter()
    {
        return Volatile.Read(ref _totalAssetCount);
    }

    // ----------------------------------------------------------- Thumbnails

    public BitmapImage? LoadThumbnail(string directoryName, string fileName, int width, int height)
    {
        bool emitUpdate = false;

        try
        {
            Folder? folder = GetFolderByPath(directoryName);

            if (folder == null)
            {
                return null;
            }

            ConcurrentDictionary<string, byte[]> thumbnails = GetOrLoadThumbnails(folder);

            if (thumbnails.TryGetValue(fileName, out byte[]? buffer))
            {
                return _imageProcessingService.LoadBitmapThumbnailImage(buffer, width, height);
            }

            // Thumbnail missing: delete orphaned asset.
            Asset? deletedAsset = RemoveAsset(directoryName, fileName);

            if (deletedAsset != null)
            {
                emitUpdate = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading thumbnail: {DirectoryName}, {FileName}", directoryName, fileName);
            throw;
        }

        if (emitUpdate)
        {
            _assetsUpdatedSubject.OnNext(Unit.Default);
        }

        return null;
    }

    // -------------------------------------------------------------- Backups

    public bool BackupExists()
    {
        return _persistenceContext.BackupExists(DateTime.Now.Date);
    }

    public void WriteBackup()
    {
        lock (_backupLock)
        {
            try
            {
                if (_persistenceContext.WriteBackup(DateTime.Now.Date))
                {
                    _persistenceContext.DeleteOldBackups(_userConfigurationService.StorageSettings.BackupsToKeep);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing backup");
                throw;
            }
        }
    }

    // -------------------------------------------------------------- Config

    public SyncAssetsConfiguration GetSyncAssetsConfiguration()
    {
        return Volatile.Read(ref _syncAssetsConfiguration);
    }

    public void SaveSyncAssetsConfiguration(SyncAssetsConfiguration syncAssetsConfiguration)
    {
        Volatile.Write(ref _syncAssetsConfiguration, syncAssetsConfiguration);

        _persistenceContext.SyncDefinitions.Replace(syncAssetsConfiguration.Definitions);
    }

    public string[] GetRecentTargetPaths()
    {
        return Volatile.Read(ref _recentTargetPaths);
    }

    public void SaveRecentTargetPaths(string[] recentTargetPaths)
    {
        Volatile.Write(ref _recentTargetPaths, recentTargetPaths);

        _persistenceContext.RecentPaths.Replace(recentTargetPaths);
    }

    public void UpdateTargetPathToRecent(Folder destinationFolder)
    {
        lock (_recentPathsLock)
        {
            string[] recentTargetPaths = Volatile.Read(ref _recentTargetPaths);

            List<string> updatedRecentTargetPaths = [destinationFolder.Path];

            for (int i = 0;
                 i < recentTargetPaths.Length && updatedRecentTargetPaths.Count < RECENT_TARGET_PATHS_MAX_COUNT;
                 i++)
            {
                if (!string.Equals(recentTargetPaths[i], destinationFolder.Path, StringComparison.Ordinal))
                {
                    updatedRecentTargetPaths.Add(recentTargetPaths[i]);
                }
            }

            string[] result = [.. updatedRecentTargetPaths];
            Volatile.Write(ref _recentTargetPaths, result);

            _persistenceContext.RecentPaths.Replace(result);
        }
    }

    // -------------------------------------------------------------- Dispose

    public void Dispose()
    {
        Volatile.Write(ref _disposed, true);
        _assetsUpdatedSubject.Dispose();
        (_persistenceContext as IDisposable)?.Dispose();
        GC.SuppressFinalize(this);
    }

    // Standard .NET IDisposable guard: prevents use-after-dispose on the Subject and persistence layer.
    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _disposed))
        {
            throw new ObjectDisposedException(nameof(AssetRepository));
        }
    }

    // -------------------------------------------------------- Initialization

    private void Initialize()
    {
        _persistenceContext.Initialize(_dataDirectory);
        ReadCatalog();
    }

    private void ReadCatalog()
    {
        try
        {
            IReadOnlyList<Folder> folders = _persistenceContext.Folders.GetAll();

            for (int i = 0; i < folders.Count; i++)
            {
                Folder folder = folders[i];

                _foldersByPath.TryAdd(folder.Path, folder);
                _foldersById.TryAdd(folder.Id, folder);
            }

            IReadOnlyList<Asset> assets = _persistenceContext.Assets.GetAll();

            int count = 0;

            for (int i = 0; i < assets.Count; i++)
            {
                Asset asset = assets[i];

                if (_foldersById.TryGetValue(asset.FolderId, out Folder? existingFolder))
                {
                    asset.Folder = existingFolder;
                }

                ConcurrentDictionary<string, Asset> folderAssets =
                    _assetsByFolderId.GetOrAdd(
                        asset.FolderId,
                        static _ => new ConcurrentDictionary<string, Asset>(StringComparer.Ordinal));

                folderAssets.TryAdd(asset.FileName, asset);

                _imageMetadataService.UpdateAssetFileProperties(asset);

                count++;
            }

            _totalAssetCount = count;

            SyncAssetsConfiguration config = new();
            config.Definitions.AddRange(_persistenceContext.SyncDefinitions.GetAll());
            _syncAssetsConfiguration = config;

            _recentTargetPaths = [.. _persistenceContext.RecentPaths.GetAll()];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading catalog");
            throw;
        }
    }

    // ------------------------------------------------------------ Internals

    private void AddAssetCore(Asset asset, byte[] thumbnailData)
    {
        if (!_foldersById.ContainsKey(asset.FolderId))
        {
            AddFolder(asset.Folder.Path);
        }

        ConcurrentDictionary<string, byte[]> thumbnails = GetOrLoadThumbnails(asset.Folder);
        thumbnails[asset.FileName] = thumbnailData;

        ConcurrentDictionary<string, Asset> folderAssets =
            _assetsByFolderId.GetOrAdd(
                asset.FolderId,
                static _ => new ConcurrentDictionary<string, Asset>(StringComparer.Ordinal));

        folderAssets[asset.FileName] = asset;
        Interlocked.Increment(ref _totalAssetCount);

        _persistenceContext.Assets.Upsert(asset);
        _assetsUpdatedSubject.OnNext(Unit.Default);
        _persistenceContext.Thumbnails.Upsert(asset.FolderId, asset.FileName, thumbnailData);
    }

    private Asset? RemoveAsset(string directory, string deletedFileName)
    {
        Folder? folder = GetFolderByPath(directory);

        if (folder == null)
        {
            return null;
        }

        if (_thumbnailCache.TryGet(folder.Id, out ConcurrentDictionary<string, byte[]> thumbnails))
        {
            thumbnails.TryRemove(deletedFileName, out _);

            if (thumbnails.IsEmpty)
            {
                _thumbnailCache.Remove(folder.Id);
                _persistenceContext.Thumbnails.DeleteByFolderId(folder.Id);
            }
            else
            {
                _persistenceContext.Thumbnails.Delete(folder.Id, deletedFileName);
            }
        }
        else
        {
            _persistenceContext.Thumbnails.Delete(folder.Id, deletedFileName);
        }

        Asset? assetToDelete = null;

        if (_assetsByFolderId.TryGetValue(folder.Id, out ConcurrentDictionary<string, Asset>? folderAssets)
            && folderAssets.TryRemove(deletedFileName, out assetToDelete))
        {
            Interlocked.Decrement(ref _totalAssetCount);
            _persistenceContext.Assets.Delete(assetToDelete.FolderId, assetToDelete.FileName);
        }

        return assetToDelete;
    }

    private Asset[] GetAssetsByFolderId(Guid folderId)
    {
        if (!_assetsByFolderId.TryGetValue(folderId, out ConcurrentDictionary<string, Asset>? folderAssets))
        {
            return [];
        }

        return [.. folderAssets.Values];
    }

    private void LoadOrFetchThumbnails(Folder folder, Asset[] assets)
    {
        ConcurrentDictionary<string, byte[]> thumbnails = GetOrLoadThumbnails(folder);

        for (int i = 0; i < assets.Length; i++)
        {
            Asset asset = assets[i];

            if (thumbnails.TryGetValue(asset.FileName, out byte[]? buffer))
            {
                asset.ImageData = _imageProcessingService.LoadBitmapThumbnailImage(
                    buffer, asset.Pixel.Thumbnail.Width, asset.Pixel.Thumbnail.Height);
            }
        }
    }

    private ConcurrentDictionary<string, byte[]> GetOrLoadThumbnails(Folder folder)
    {
        if (_thumbnailCache.TryGet(folder.Id, out ConcurrentDictionary<string, byte[]> thumbnailsCached))
        {
            return thumbnailsCached;
        }

        Dictionary<string, byte[]> savedThumbnails = _persistenceContext.Thumbnails.GetByFolderId(folder.Id);

        ConcurrentDictionary<string, byte[]> thumbnails =
            savedThumbnails.Count == 0
                ? new ConcurrentDictionary<string, byte[]>(StringComparer.Ordinal)
                : new ConcurrentDictionary<string, byte[]>(savedThumbnails, StringComparer.Ordinal);

        _thumbnailCache.Set(folder.Id, thumbnails);

        return thumbnails;
    }

    private static Asset[] FilterAssetsWithImageData(Asset[] assets)
    {
        int validCount = 0;

        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i].ImageData != null)
            {
                validCount++;
            }
        }

        if (validCount == assets.Length)
        {
            return assets;
        }

        Asset[] filterAssetsWithImageData = new Asset[validCount];
        int index = 0;

        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i].ImageData != null)
            {
                filterAssetsWithImageData[index++] = assets[i];
            }
        }

        return filterAssetsWithImageData;
    }
}
