using Microsoft.Extensions.Logging;
using PhotoManager.Domain.Interfaces.Persistence;
using PhotoManager.Persistence.Cache;
using System.Collections.Concurrent;
using System.Reactive;

namespace PhotoManager.Infrastructure;

// ReSharper disable InconsistentlySynchronizedField
public class AssetRepository : IAssetRepository, IDisposable
{
    private const int RECENT_TARGET_PATHS_MAX_COUNT = 20;

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
    private readonly Lock _assetMutationLock = new();
    private readonly Lock _recentPathsLock = new();
    private readonly Lock _backupLock = new();
    private readonly AssetsUpdatedObservable _assetsUpdatedObservable = new();
    private bool _disposed;

    public IObservable<Unit> AssetsUpdated => _assetsUpdatedObservable;

    /// <summary>
    /// The persistence context is received already initialized (the composition root owns its bootstrap); this
    /// repository only reads the catalog into memory and never initializes the database itself.
    /// </summary>
    public AssetRepository(
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

        ushort cacheCapacity = userConfigurationService.StorageSettings.ThumbnailsDictionaryEntriesToKeep;

        if (cacheCapacity == 0)
        {
            cacheCapacity = 1;
        }

        _thumbnailCache = new LruCache<Guid, ConcurrentDictionary<string, byte[]>>(cacheCapacity);

        ReadCatalog();
    }

    // -------------------------------------------------------------- Folders

    public Folder AddFolder(string path)
    {
        ThrowIfDisposed();

        lock (_folderCreationLock)
        {
            if (_foldersByPath.TryGetValue(path, out Folder? existingFolder))
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
        return CopyToArray(_foldersById.Values);
    }

    public HashSet<string> GetFoldersPath()
    {
        return new(_foldersByPath.Keys, StringComparer.Ordinal);
    }

    public Folder[] GetSubFolders(Folder parentFolder)
    {
        List<Folder> result = [];

        foreach (Folder folder in _foldersById.Values)
        {
            if (parentFolder.IsParentOf(folder))
            {
                result.Add(folder);
            }
        }

        return CopyToArray(result);
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

            int removedAssetCount = 0;

            lock (_assetMutationLock)
            {
                _persistenceContext.DeleteFolderWithAssetsAndThumbnails(folder.Id);

                if (_assetsByFolderId.TryRemove(folder.Id, out ConcurrentDictionary<string, Asset>? folderAssets))
                {
                    removedAssetCount = folderAssets.Count;
                    Interlocked.Add(ref _totalAssetCount, -removedAssetCount);
                    DisposeAssetImageData(folderAssets.Values);
                }

                _thumbnailCache.Remove(folder.Id);

                if (_foldersByPath.TryGetValue(folder.Path, out Folder? folderExisting)
                    && folderExisting.Id == folder.Id)
                {
                    _foldersByPath.TryRemove(folder.Path, out _);
                }

                _foldersById.TryRemove(folder.Id, out _);
            }

            if (removedAssetCount > 0)
            {
                NotifyAssetsUpdated();
            }
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

            ArgumentNullException.ThrowIfNull(thumbnailData);

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

    public int AddAssets(IReadOnlyList<AssetWithThumbnail> assetsWithThumbnails)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(assetsWithThumbnails);

        if (assetsWithThumbnails.Count == 0)
        {
            return 0;
        }

        try
        {
            for (int i = 0; i < assetsWithThumbnails.Count; i++)
            {
                AssetWithThumbnail assetWithThumbnail = assetsWithThumbnails[i];

                if (string.IsNullOrWhiteSpace(assetWithThumbnail.Asset.Folder.Path))
                {
                    _logger.LogError(
                        "The asset could not be added, folder path is null or empty, asset.FileName: {FileName}",
                        assetWithThumbnail.Asset.FileName);
                    return 0;
                }

                ArgumentNullException.ThrowIfNull(assetWithThumbnail.ThumbnailData);
            }

            AssetWithThumbnail[] canonicalAssetsWithThumbnails;

            lock (_assetMutationLock)
            {
                canonicalAssetsWithThumbnails = new AssetWithThumbnail[assetsWithThumbnails.Count];

                for (int i = 0; i < assetsWithThumbnails.Count; i++)
                {
                    AssetWithThumbnail assetWithThumbnail = assetsWithThumbnails[i];
                    Asset canonicalAsset = GetAssetWithCanonicalFolder(assetWithThumbnail.Asset);
                    canonicalAssetsWithThumbnails[i] = assetWithThumbnail with { Asset = canonicalAsset };
                }

                _persistenceContext.UpsertAssetsWithThumbnails(canonicalAssetsWithThumbnails);

                for (int i = 0; i < canonicalAssetsWithThumbnails.Length; i++)
                {
                    AssetWithThumbnail assetWithThumbnail = canonicalAssetsWithThumbnails[i];
                    AddAssetToMemory(assetWithThumbnail.Asset, assetWithThumbnail.ThumbnailData);
                }
            }

            NotifyAssetsUpdated();

            return canonicalAssetsWithThumbnails.Length;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding asset batch: {AssetCount}", assetsWithThumbnails.Count);
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
            NotifyAssetsUpdated();
        }

        return deletedAsset;
    }

    public IReadOnlyList<Asset> DeleteAssets(string directory, IReadOnlyList<string> deletedFileNames)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(deletedFileNames);

        if (deletedFileNames.Count == 0)
        {
            return [];
        }

        try
        {
            Folder? folder = GetFolderByPath(directory);

            if (folder == null)
            {
                return [];
            }

            List<Asset> deletedAssets;

            lock (_assetMutationLock)
            {
                _persistenceContext.DeleteAssetsWithThumbnails(folder.Id, deletedFileNames);
                deletedAssets = RemoveAssetsFromMemory(folder, deletedFileNames);
            }

            if (deletedAssets.Count > 0)
            {
                NotifyAssetsUpdated();
            }

            return deletedAssets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting asset batch: {Directory}, {AssetCount}",
                directory, deletedFileNames.Count);
            throw;
        }
    }

    public Asset[] GetCataloguedAssets()
    {
        List<Asset> result = new(Math.Max(0, Volatile.Read(ref _totalAssetCount)));

        foreach (ConcurrentDictionary<string, Asset> folderAssets in _assetsByFolderId.Values)
        {
            result.AddRange(folderAssets.Values);
        }

        return CopyToArray(result);
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

        return CopyToArray(folderAssets.Values);
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

    public IImageData? LoadThumbnail(string directoryName, string fileName, int width, int height)
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
                return _imageProcessingService.LoadBitmapThumbnailImage(buffer, ImageRotation.Rotate0, width, height);
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
            NotifyAssetsUpdated();
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

            string[] result = CopyToArray(updatedRecentTargetPaths);
            Volatile.Write(ref _recentTargetPaths, result);

            _persistenceContext.RecentPaths.Replace(result);
        }
    }

    // ----------------------------------------------------------- Stored state

    public string? GetStoredAssetsDirectory()
    {
        try
        {
            ThrowIfDisposed();

            return _persistenceContext.Configuration.GetValue(ConfigurationKeys.ASSETS_DIRECTORY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stored assets directory");
            throw;
        }
    }

    public void StoreAssetsDirectory(string path)
    {
        try
        {
            ThrowIfDisposed();

            _persistenceContext.Configuration.SetValue(ConfigurationKeys.ASSETS_DIRECTORY, path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing assets directory: {Path}", path);
            throw;
        }
    }

    public void Vacuum()
    {
        try
        {
            ThrowIfDisposed();

            _persistenceContext.Vacuum();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error vacuuming the database");
            throw;
        }
    }

    // -------------------------------------------------------------- Dispose

    public void Dispose()
    {
        Volatile.Write(ref _disposed, true);
        DisposeAssetImageData(_assetsByFolderId.Values.SelectMany(static folderAssets => folderAssets.Values));
        _assetsUpdatedObservable.Dispose();
        (_persistenceContext as IDisposable)?.Dispose();
        GC.SuppressFinalize(this);
    }

    // Standard .NET IDisposable guard: prevents use-after-dispose on the Subject and persistence layer
    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _disposed))
        {
            throw new ObjectDisposedException(nameof(AssetRepository));
        }
    }

    // -------------------------------------------------------- Initialization

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

            ParallelOptions readCatalogOptions = new()
            {
                MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 8)
            };

            Parallel.For(0, assets.Count, readCatalogOptions, i => AddCatalogAssetToMemory(assets[i]));

            _totalAssetCount = assets.Count;

            SyncAssetsConfiguration config = new();
            config.Definitions.AddRange(_persistenceContext.SyncDefinitions.GetAll());
            _syncAssetsConfiguration = config;

            _recentTargetPaths = CopyReadOnlyListToArray(_persistenceContext.RecentPaths.GetAll());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading catalog");
            throw;
        }
    }

    private void AddCatalogAssetToMemory(Asset asset)
    {
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
    }

    // ------------------------------------------------------------ Internals

    private void AddAssetCore(Asset asset, byte[] thumbnailData)
    {
        lock (_assetMutationLock)
        {
            Asset canonicalAsset = GetAssetWithCanonicalFolder(asset);
            _persistenceContext.UpsertAssetWithThumbnail(canonicalAsset, thumbnailData);
            AddAssetToMemory(canonicalAsset, thumbnailData);
        }

        NotifyAssetsUpdated();
    }

    private Asset GetAssetWithCanonicalFolder(Asset asset)
    {
        if (_foldersById.TryGetValue(asset.FolderId, out Folder? existingFolderById))
        {
            asset.Folder = existingFolderById;
            return asset;
        }

        AddFolder(asset.Folder.Path);

        return asset;
    }

    private void AddAssetToMemory(Asset asset, byte[] thumbnailData)
    {
        ConcurrentDictionary<string, Asset> folderAssets =
            _assetsByFolderId.GetOrAdd(
                asset.FolderId,
                static _ => new ConcurrentDictionary<string, Asset>(StringComparer.Ordinal));

        lock (folderAssets)
        {
            if (folderAssets.TryGetValue(asset.FileName, out Asset? existingAsset))
            {
                if (!ReferenceEquals(existingAsset, asset))
                {
                    folderAssets[asset.FileName] = asset;
                    DisposeAssetImageData(existingAsset);
                }
            }
            else
            {
                folderAssets[asset.FileName] = asset;
                Interlocked.Increment(ref _totalAssetCount);
            }

            ConcurrentDictionary<string, byte[]> thumbnails = GetOrLoadThumbnails(asset.Folder);
            thumbnails[asset.FileName] = thumbnailData;
        }
    }

    private Asset? RemoveAsset(string directory, string deletedFileName)
    {
        Folder? folder = GetFolderByPath(directory);

        if (folder == null)
        {
            return null;
        }

        lock (_assetMutationLock)
        {
            _persistenceContext.DeleteAssetWithThumbnail(folder.Id, deletedFileName);

            if (!_assetsByFolderId.TryGetValue(folder.Id, out ConcurrentDictionary<string, Asset>? folderAssets))
            {
                return null;
            }

            Asset? assetToDelete;

            lock (folderAssets)
            {
                if (!folderAssets.TryRemove(deletedFileName, out assetToDelete))
                {
                    return null;
                }

                RemoveThumbnailFromCache(folder.Id, deletedFileName);
                Interlocked.Decrement(ref _totalAssetCount);
            }

            DisposeAssetImageData(assetToDelete);
            return assetToDelete;
        }
    }

    private List<Asset> RemoveAssetsFromMemory(Folder folder, IReadOnlyList<string> deletedFileNames)
    {
        List<Asset> deletedAssets = new(deletedFileNames.Count);

        if (!_assetsByFolderId.TryGetValue(folder.Id, out ConcurrentDictionary<string, Asset>? folderAssets))
        {
            return deletedAssets;
        }

        lock (folderAssets)
        {
            for (int i = 0; i < deletedFileNames.Count; i++)
            {
                if (folderAssets.TryRemove(deletedFileNames[i], out Asset? deletedAsset))
                {
                    RemoveThumbnailFromCache(folder.Id, deletedFileNames[i]);
                    Interlocked.Decrement(ref _totalAssetCount);
                    deletedAssets.Add(deletedAsset);
                }
            }
        }

        for (int i = 0; i < deletedAssets.Count; i++)
        {
            DisposeAssetImageData(deletedAssets[i]);
        }

        return deletedAssets;
    }

    private void RemoveThumbnailFromCache(Guid folderId, string fileName)
    {
        if (!_thumbnailCache.TryGet(folderId, out ConcurrentDictionary<string, byte[]> thumbnails))
        {
            return;
        }

        thumbnails.TryRemove(fileName, out _);

        if (thumbnails.IsEmpty)
        {
            _thumbnailCache.Remove(folderId);
        }
    }

    private Asset[] GetAssetsByFolderId(Guid folderId)
    {
        if (!_assetsByFolderId.TryGetValue(folderId, out ConcurrentDictionary<string, Asset>? folderAssets))
        {
            return [];
        }

        return CopyToArray(folderAssets.Values);
    }

    private void LoadOrFetchThumbnails(Folder folder, Asset[] assets)
    {
        ConcurrentDictionary<string, byte[]> thumbnails = GetOrLoadThumbnails(folder);

        for (int i = 0; i < assets.Length; i++)
        {
            Asset asset = assets[i];

            if (thumbnails.TryGetValue(asset.FileName, out byte[]? buffer))
            {
                // Stored thumbnail bytes were already orientation-normalized when the asset was created
                IImageData imageData = _imageProcessingService.LoadBitmapThumbnailImage(
                    buffer, ImageRotation.Rotate0, asset.Pixel.Thumbnail.Width, asset.Pixel.Thumbnail.Height);
                asset.ImageData?.Dispose();
                asset.ImageData = imageData;
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

    private static void DisposeAssetImageData(IEnumerable<Asset> assets)
    {
        foreach (Asset asset in assets)
        {
            DisposeAssetImageData(asset);
        }
    }

    private static void DisposeAssetImageData(Asset asset)
    {
        asset.ImageData?.Dispose();
        asset.ImageData = null;
    }

    private void NotifyAssetsUpdated()
    {
        _assetsUpdatedObservable.Notify();
    }

    private sealed class AssetsUpdatedObservable : IObservable<Unit>, IDisposable
    {
        private readonly Lock _lock = new();
        private readonly List<IObserver<Unit>> _observers = [];
        private bool _disposed;

        public IDisposable Subscribe(IObserver<Unit> observer)
        {
            ArgumentNullException.ThrowIfNull(observer);

            lock (_lock)
            {
                if (_disposed)
                {
                    observer.OnCompleted();
                    return EmptyDisposable.Instance;
                }

                _observers.Add(observer);
            }

            return new Subscription(this, observer);
        }

        public void Notify()
        {
            lock (_lock)
            {
                for (int i = 0; i < _observers.Count; i++)
                {
                    _observers[i].OnNext(Unit.Default);
                }
            }
        }

        public void Dispose()
        {
            IObserver<Unit>[] observers;

            lock (_lock)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                observers = CopyToArray(_observers);
                _observers.Clear();
            }

            for (int i = 0; i < observers.Length; i++)
            {
                observers[i].OnCompleted();
            }
        }

        private void Unsubscribe(IObserver<Unit> observer)
        {
            lock (_lock)
            {
                _observers.Remove(observer);
            }
        }

        private sealed class Subscription(AssetsUpdatedObservable owner, IObserver<Unit> observer) : IDisposable
        {
            private AssetsUpdatedObservable? _owner = owner;

            public void Dispose()
            {
                AssetsUpdatedObservable? currentOwner = Interlocked.Exchange(ref _owner, null);
                currentOwner?.Unsubscribe(observer);
            }
        }

        private sealed class EmptyDisposable : IDisposable
        {
            public static readonly EmptyDisposable Instance = new();

            public void Dispose()
            {
            }
        }
    }

    private static T[] CopyToArray<T>(ICollection<T> values)
    {
        T[] result = new T[values.Count];
        values.CopyTo(result, 0);
        return result;
    }

    private static T[] CopyReadOnlyListToArray<T>(IReadOnlyList<T> values)
    {
        T[] result = new T[values.Count];

        for (int i = 0; i < values.Count; i++)
        {
            result[i] = values[i];
        }

        return result;
    }
}
