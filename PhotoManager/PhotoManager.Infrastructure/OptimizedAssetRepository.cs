using Microsoft.Extensions.Logging;
using PhotoManager.Persistence;
using PhotoManager.Persistence.Cache;
using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PhotoManager.Infrastructure;

public sealed class OptimizedAssetRepository : IOptimizedAssetRepository
{
    private const int RECENT_TARGET_PATHS_MAX_COUNT = 20;

    private readonly string _dataDirectory;
    private int _totalAssetCount;
    private string[] _recentTargetPaths = [];

    private readonly IPersistenceContext _persistence;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IImageMetadataService _imageMetadataService;
    private readonly IUserConfigurationService _userConfigurationService;
    private readonly ILogger<OptimizedAssetRepository> _logger;

    private readonly ConcurrentDictionary<string, Folder> _foldersByPath = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<Guid, Folder> _foldersById = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, Asset>> _assetsByFolderId = new();
    private readonly Dictionary<string, List<Asset>> _assetsByHash = new(StringComparer.Ordinal);

    private readonly LruCache<Guid, ConcurrentDictionary<string, byte[]>> _thumbnailCache;

    private SyncAssetsConfiguration _syncAssetsConfiguration = new();

    private readonly Lock _folderCreationLock = new();
    private readonly Lock _hashIndexLock = new();
    private readonly Lock _recentPathsLock = new();

    private readonly Subject<Unit> _assetsUpdatedSubject = new();
    private bool _disposed;

    public IObservable<Unit> AssetsUpdated => _assetsUpdatedSubject.AsObservable();

    public OptimizedAssetRepository(
        IPersistenceContext persistence,
        IPathProviderService pathProviderService,
        IImageProcessingService imageProcessingService,
        IImageMetadataService imageMetadataService,
        IUserConfigurationService userConfigurationService,
        ILogger<OptimizedAssetRepository> logger)
    {
        _persistence = persistence;
        _imageProcessingService = imageProcessingService;
        _imageMetadataService = imageMetadataService;
        _userConfigurationService = userConfigurationService;
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

            Folder folder = _persistence.Folders.Insert(path);

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
                lock (_hashIndexLock)
                {
                    foreach (Asset asset in folderAssets.Values)
                    {
                        if (_assetsByHash.TryGetValue(asset.Hash, out List<Asset>? bucket))
                        {
                            bucket.Remove(asset);

                            if (bucket.Count == 0)
                            {
                                _assetsByHash.Remove(asset.Hash);
                            }
                        }
                    }
                }

                Interlocked.Add(ref _totalAssetCount, -folderAssets.Count);
            }

            _persistence.Assets.DeleteByFolderId(folder.Id);
            _persistence.Thumbnails.DeleteByFolderId(folder.Id);
            _persistence.Folders.Delete(folder.Id);

            if (_foldersByPath.TryGetValue(folder.Path, out Folder? folderExisting) && folderExisting.Id == folder.Id)
            {
                _foldersByPath.TryRemove(folder.Path, out _);
            }

            _foldersById.TryRemove(folder.Id, out _);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting folder: {FolderPath}", folder.Path);
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

        if (string.IsNullOrWhiteSpace(asset.Folder.Path))
        {
            _logger.LogError("The asset could not be added, folder path is null or empty, asset.FileName: {FileName}",
                asset.FileName);
            return false;
        }

        try
        {
            AddAssetCore(asset, thumbnailData);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding asset: {FileName}, FolderId: {FolderId}",
                asset.FileName, asset.FolderId);
            return false;
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

        Asset[] assets = [.. folderAssets.Values];

        // TODO: Why do we need this sort ?
        Array.Sort(assets, static (a, b) => string.Compare(a.FileName, b.FileName, StringComparison.Ordinal));

        return assets;
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
        return _persistence.BackupExists(DateTime.Now.Date);
    }

    public void WriteBackup()
    {
        try
        {
            if (_persistence.WriteBackup(DateTime.Now.Date))
            {
                _persistence.DeleteOldBackups(_userConfigurationService.StorageSettings.BackupsToKeep);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing backup");
            throw;
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

        _persistence.SyncDefinitions.Replace(syncAssetsConfiguration.Definitions);
    }

    public string[] GetRecentTargetPaths()
    {
        return Volatile.Read(ref _recentTargetPaths);
    }

    public void SaveRecentTargetPaths(string[] recentTargetPaths)
    {
        Volatile.Write(ref _recentTargetPaths, recentTargetPaths);

        _persistence.RecentPaths.Replace(recentTargetPaths);
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

            _persistence.RecentPaths.Replace(result);
        }
    }

    // -------------------------------------------------------------- Dispose

    public void Dispose()
    {
        Volatile.Write(ref _disposed, true);
        _assetsUpdatedSubject.Dispose();
    }

    // Standard .NET IDisposable guard: prevents use-after-dispose on the Subject and persistence layer.
    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _disposed))
        {
            throw new ObjectDisposedException(nameof(OptimizedAssetRepository));
        }
    }

    // -------------------------------------------------------- Initialization

    private void Initialize()
    {
        _persistence.Initialize(_dataDirectory);
        ReadCatalog();
    }

    private void ReadCatalog()
    {
        try
        {
            IReadOnlyList<Folder> folders = _persistence.Folders.GetAll();

            for (int i = 0; i < folders.Count; i++)
            {
                Folder folder = folders[i];

                _foldersByPath.TryAdd(folder.Path, folder);
                _foldersById.TryAdd(folder.Id, folder);
            }

            IReadOnlyList<Asset> assets = _persistence.Assets.GetAll();

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

                if (!_assetsByHash.TryGetValue(asset.Hash, out List<Asset>? assetsByHash))
                {
                    assetsByHash = [];
                    _assetsByHash[asset.Hash] = assetsByHash;
                }

                assetsByHash.Add(asset);

                _imageMetadataService.UpdateAssetFileProperties(asset);

                count++;
            }

            _totalAssetCount = count;

            SyncAssetsConfiguration config = new();
            config.Definitions.AddRange(_persistence.SyncDefinitions.GetAll());
            _syncAssetsConfiguration = config;

            _recentTargetPaths = [.. _persistence.RecentPaths.GetAll()];
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

        lock (_hashIndexLock)
        {
            if (!_assetsByHash.TryGetValue(asset.Hash, out List<Asset>? assetsByHash))
            {
                assetsByHash = [];
                _assetsByHash[asset.Hash] = assetsByHash;
            }

            assetsByHash.Add(asset);
        }

        _persistence.Assets.Upsert(asset);
        _persistence.Thumbnails.Upsert(asset.FolderId, asset.FileName, thumbnailData);

        _assetsUpdatedSubject.OnNext(Unit.Default);
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
                _persistence.Thumbnails.DeleteByFolderId(folder.Id);
            }
            else
            {
                _persistence.Thumbnails.Delete(folder.Id, deletedFileName);
            }
        }
        else
        {
            _persistence.Thumbnails.Delete(folder.Id, deletedFileName);
        }

        Asset? assetToDelete = null;

        if (_assetsByFolderId.TryGetValue(folder.Id, out ConcurrentDictionary<string, Asset>? folderAssets)
            && folderAssets.TryRemove(deletedFileName, out assetToDelete))
        {
            Interlocked.Decrement(ref _totalAssetCount);
            DeleteAssetInternal(assetToDelete);
        }

        return assetToDelete;
    }

    private void DeleteAssetInternal(Asset asset)
    {
        lock (_hashIndexLock)
        {
            if (_assetsByHash.TryGetValue(asset.Hash, out List<Asset>? assetsByHash))
            {
                assetsByHash.Remove(asset);

                if (assetsByHash.Count == 0)
                {
                    _assetsByHash.Remove(asset.Hash);
                }
            }
        }

        _persistence.Assets.Delete(asset.FolderId, asset.FileName);
    }

    private Asset[] GetAssetsByFolderId(Guid folderId)
    {
        if (!_assetsByFolderId.TryGetValue(folderId, out ConcurrentDictionary<string, Asset>? folderAssets))
        {
            return [];
        }

        Asset[] assets = [.. folderAssets.Values];

        // TODO: Why do we need this sort ?
        Array.Sort(assets, static (a, b) => string.Compare(a.FileName, b.FileName, StringComparison.Ordinal));

        return assets;
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

        Dictionary<string, byte[]> savedThumbnails = _persistence.Thumbnails.GetByFolderId(folder.Id);

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
