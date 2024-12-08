using log4net;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;

namespace PhotoManager.Infrastructure;

public class AssetRepository : IAssetRepository
{
    private const int RECENT_TARGET_PATHS_MAX_COUNT = 20;

    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public bool IsInitialized { get; private set; }
    private readonly string dataDirectory;
    private readonly IDatabase _database;
    private readonly IStorageService _storageService;
    private readonly IUserConfigurationService _userConfigurationService;

    private List<Asset> assets;
    private List<Folder> folders;
    private SyncAssetsConfiguration syncAssetsConfiguration;
    private List<string> recentTargetPaths;
    protected Dictionary<string, Dictionary<string, byte[]>> Thumbnails { get; private set; }
    private readonly Queue<string> recentThumbnailsQueue;
    private bool hasChanges;
    private readonly object syncLock;
    private readonly Subject<Unit> _assetsUpdatedSubject = new();
    public IObservable<Unit> AssetsUpdated => _assetsUpdatedSubject.AsObservable();

    public AssetRepository(IDatabase database, IStorageService storageService, IUserConfigurationService userConfigurationService)
    {
        _database = database;
        _storageService = storageService;
        _userConfigurationService = userConfigurationService;
        assets = [];
        folders = [];
        syncAssetsConfiguration = new SyncAssetsConfiguration();
        recentTargetPaths = [];
        recentThumbnailsQueue = new Queue<string>();
        Thumbnails = [];
        syncLock = new object();
        dataDirectory = _storageService.ResolveDataDirectory(_userConfigurationService.StorageSettings.StorageVersion);
        Initialize();
    }

    public Asset[] GetAssetsByPath(string directory)
    {
        List<Asset> assetsList = []; // TODO: Why array at the end ?
        bool isNewFile = false;

        try
        {
            lock (syncLock)
            {
                Folder? folder = GetFolderByPath(directory);

                if (folder != null)
                {
                    assetsList = GetAssetsByFolderId(folder.FolderId);

                    if (!Thumbnails.ContainsKey(folder.Path))
                    {
                        Thumbnails[folder.Path] = GetThumbnails(folder, out isNewFile);
                        RemoveOldThumbnailsDictionaryEntries(folder);
                    }

                    if (!isNewFile)
                    {
                        foreach (Asset asset in assetsList)
                        {
                            if (Thumbnails.TryGetValue(folder.Path, out Dictionary<string, byte[]>? thumbnail) && thumbnail.ContainsKey(asset.FileName))
                            {
                                asset.ImageData = _storageService.LoadBitmapThumbnailImage(thumbnail[asset.FileName], asset.Pixel.Thumbnail.Width, asset.Pixel.Thumbnail.Height);
                            }
                        }

                        // Removes assets with no thumbnails
                        assetsList.RemoveAll(asset => asset.ImageData == null);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }

        return assetsList.ToArray();
    }

    // TODO: Return Asset created
    public void AddAsset(Asset asset, byte[] thumbnailData)
    {
        lock (syncLock)
        {
            Folder? folder = GetFolderById(asset.FolderId);

            if (string.IsNullOrWhiteSpace(asset.Folder.Path))
            {
                return; // TODO: log.Error($"The asset could not be added, folder is null, asset.FileName: {asset.FileName}");
            }

            if (folder == null)
            {
                AddFolder(asset.Folder.Path);
            }

            if (!Thumbnails.ContainsKey(asset.Folder.Path))
            {
                Thumbnails[asset.Folder.Path] = GetThumbnails(asset.Folder, out _);
                RemoveOldThumbnailsDictionaryEntries(asset.Folder);
            }

            if (Thumbnails.TryGetValue(asset.Folder.Path, out Dictionary<string, byte[]>? folderThumbnails))
            {
                folderThumbnails[asset.FileName] = thumbnailData;
                assets.Add(asset);
                hasChanges = true;
                _assetsUpdatedSubject.OnNext(Unit.Default);
            }
        }
    }

    public Folder AddFolder(string path) // Play this before anything else to register every folder
    {
        Folder folder;

        lock (syncLock)
        {
            folder = new Folder
            {
                FolderId = Guid.NewGuid(),
                Path = path
            };

            folders.Add(folder);
            hasChanges = true;
        }

        return folder;
    }

    public bool FolderExists(string path)
    {
        bool result;

        lock (syncLock)
        {
            result = folders.Any(f => f.Path == path);
        }

        return result;
    }

    public Folder[] GetFolders()
    {
        Folder[] result;

        lock (syncLock)
        {
            result = [..folders];
        }

        return result;
    }

    // TODO: Is HashSet the right thing to do ? (Because it does not preserve the order)
    public HashSet<string> GetFoldersPath()
    {
        HashSet<string> folderPaths;

        lock (syncLock)
        {
            folderPaths = folders.Select(folder => folder.Path).ToHashSet();
        }

        return folderPaths;
    }

    public Folder[] GetSubFolders(Folder parentFolder, bool includeHidden) // TODO: Remove includeHidden when tests done in the caller class
    {
        return folders.Where(parentFolder.IsParentOf).ToArray();
    }

    public Folder? GetFolderByPath(string path)
    {
        Folder? result;

        lock (syncLock)
        {
            result = folders.FirstOrDefault(f => f.Path == path);
        }

        return result;
    }

    public void SaveCatalog(Folder? folder)
    {
        lock (syncLock)
        {
            if (hasChanges)
            {
                WriteAssets(assets);
                WriteFolders(folders);
                WriteSyncAssetsDirectoriesDefinitions(syncAssetsConfiguration.Definitions);
                WriteRecentTargetPaths(recentTargetPaths);

                hasChanges = false;
            }

            if (folder != null && Thumbnails.ContainsKey(folder.Path))
            {
                SaveThumbnails(Thumbnails[folder.Path], folder.ThumbnailsFilename);
            }
        }
    }

    public bool BackupExists()
    {
        lock (syncLock)
        {
            return _database.BackupExists(DateTime.Now.Date);
        }
    }

    public void WriteBackup()
    {
        lock (syncLock)
        {
            if (_database.WriteBackup(DateTime.Now.Date))
            {
                _database.DeleteOldBackups(_userConfigurationService.StorageSettings.BackupsToKeep);
            }
        }
    }

    public List<Asset> GetCataloguedAssets()
    {
        List<Asset>? cataloguedAssets;

        lock (syncLock)
        {
            cataloguedAssets = assets;
        }

        return cataloguedAssets;
    }

    // TODO: Improve it by having a Dict instead Dictionary<string, List<Asset>>
    public List<Asset> GetCataloguedAssetsByPath(string directory)
    {
        List<Asset> cataloguedAssets = [];

        lock (syncLock)
        {
            Folder? folder = GetFolderByPath(directory);

            if (folder != null)
            {
                cataloguedAssets = assets.Where(a => a.FolderId == folder.FolderId).ToList();
            }
        }

        return cataloguedAssets;
    }

    public bool IsAssetCatalogued(string directoryName, string fileName)
    {
        bool result;

        lock (syncLock)
        {
            Folder? folder = GetFolderByPath(directoryName);
            result = folder != null && GetAssetByFolderIdAndFileName(folder.FolderId, fileName) != null;
        }

        return result;
    }

    public Asset? DeleteAsset(string directory, string fileName)
    {
        lock (syncLock)
        {
            Folder? folder = GetFolderByPath(directory);

            if (folder != null)
            {
                Asset? assetToDelete = GetAssetByFolderIdAndFileName(folder.FolderId, fileName);

                if (!Thumbnails.ContainsKey(folder.Path))
                {
                    Thumbnails[folder.Path] = GetThumbnails(folder, out _);
                    RemoveOldThumbnailsDictionaryEntries(folder);
                }

                if (Thumbnails.TryGetValue(folder.Path, out Dictionary<string, byte[]>? thumbnail))
                {
                    thumbnail.Remove(fileName);
                }

                //if (Thumbnails[folder.Path].Count == 0) // TODO: when tested above, uncomment it -> to clean up Thumbnails that containing 0 values, no need to store these Thumbnails
                //{
                //    Thumbnails.Remove(folder.Path);
                //}

                if (assetToDelete != null)
                {
                    assets.Remove(assetToDelete);
                    hasChanges = true;
                    _assetsUpdatedSubject.OnNext(Unit.Default);
                }

                return assetToDelete;
            }

            return null;
        }
    }

    public void DeleteFolder(Folder folder)
    {
        lock (syncLock)
        {
            Thumbnails.Remove(folder.Path);

            if (FolderHasThumbnails(folder))
            {
                _database.DeleteThumbnails(folder.ThumbnailsFilename);
            }

            folders.Remove(folder);
            hasChanges = true;
        }
    }

    public bool HasChanges()
    {
        bool result;

        lock (syncLock)
        {
            result = hasChanges;
        }

        return result;
    }

    // TODO: Seems to be a dead method
    public bool ContainsThumbnail(string directoryName, string fileName)
    {
        bool result;

        lock (syncLock)
        {
            if (!Thumbnails.ContainsKey(directoryName))
            {
                Folder? folder = GetFolderByPath(directoryName);
                Thumbnails[directoryName] = GetThumbnails(folder, out _);
                RemoveOldThumbnailsDictionaryEntries(folder);
            }

            result = !string.IsNullOrEmpty(fileName) && Thumbnails.TryGetValue(directoryName, out Dictionary<string, byte[]>? thumbnail) && thumbnail.ContainsKey(fileName);
        }

        return result;
    }

    public BitmapImage? LoadThumbnail(string directoryName, string fileName, int width, int height)
    {
        BitmapImage? result = null;

        lock (syncLock)
        {
            if (!Thumbnails.ContainsKey(directoryName))
            {
                Folder? folder = GetFolderByPath(directoryName);
                Thumbnails[directoryName] = GetThumbnails(folder, out _);
                RemoveOldThumbnailsDictionaryEntries(folder);
            }

            if (Thumbnails.TryGetValue(directoryName, out Dictionary<string, byte[]>? thumbnail) && thumbnail.ContainsKey(fileName))
            {
                result = _storageService.LoadBitmapThumbnailImage(thumbnail[fileName], width, height);
            }
            else
            {
                _ = DeleteAsset(directoryName, fileName);
                Folder? folder = GetFolderByPath(directoryName);
                SaveCatalog(folder);
            }
        }

        return result;
    }

    // TODO: Rename to FolderHasBlobs or something like this (Thumbnails has been used too much wrongly)
    public bool FolderHasThumbnails(Folder folder)
    {
        return _database.FolderHasThumbnails(folder.ThumbnailsFilename);
    }

    public SyncAssetsConfiguration GetSyncAssetsConfiguration()
    {
        SyncAssetsConfiguration result;

        lock (syncLock)
        {
            result = syncAssetsConfiguration;
        }

        return result;
    }

    public void SaveSyncAssetsConfiguration(SyncAssetsConfiguration syncAssetsConfig)
    {
        lock (syncLock)
        {
            syncAssetsConfiguration = syncAssetsConfig;
            hasChanges = true;
        }
    }

    public List<string> GetRecentTargetPaths()
    {
        List<string> result;

        lock (syncLock)
        {
            result = recentTargetPaths;
        }

        return result;
    }

    public void SaveRecentTargetPaths(List<string> paths)
    {
        lock (syncLock)
        {
            recentTargetPaths = paths;
            hasChanges = true;
        }
    }

    public void UpdateTargetPathToRecent(Folder destinationFolder)
    {
        lock (syncLock)
        {
            List<string> recentTargetPathsUpdated = [..recentTargetPaths];

            if (recentTargetPathsUpdated.Contains(destinationFolder.Path))
            {
                recentTargetPathsUpdated.Remove(destinationFolder.Path);
            }

            recentTargetPathsUpdated.Insert(0, destinationFolder.Path);

            recentTargetPathsUpdated = recentTargetPathsUpdated.Take(RECENT_TARGET_PATHS_MAX_COUNT).ToList();

            SaveRecentTargetPaths(recentTargetPathsUpdated);
        }
    }

    public int GetAssetsCounter()
    {
        lock (syncLock)
        {
            return assets.Count;
        }
    }

    #region private
    private void Initialize()
    {
        if (!IsInitialized)
        {
            InitializeDatabase();
            ReadCatalog();
            IsInitialized = true;
        }
    }

    private void InitializeDatabase()
    {
        _database.Initialize(
            dataDirectory,
            _userConfigurationService.StorageSettings.Separator,
            _userConfigurationService.StorageSettings.FoldersNameSettings.Tables,
            _userConfigurationService.StorageSettings.FoldersNameSettings.Blobs);

        _database.SetDataTableProperties(new DataTableProperties
        {
            TableName = _userConfigurationService.StorageSettings.TablesSettings.FoldersTableName,
            ColumnProperties = FolderConfigs.ConfigureDataTable()
        });

        _database.SetDataTableProperties(new DataTableProperties
        {
            TableName = _userConfigurationService.StorageSettings.TablesSettings.AssetsTableName,
            ColumnProperties = AssetConfigs.ConfigureDataTable()
        });

        _database.SetDataTableProperties(new DataTableProperties
        {
            TableName = _userConfigurationService.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName,
            ColumnProperties = SyncAssetsDirectoriesDefinitionConfigs.ConfigureDataTable()
        });

        _database.SetDataTableProperties(new DataTableProperties
        {
            TableName = _userConfigurationService.StorageSettings.TablesSettings.RecentTargetPathsTableName,
            ColumnProperties = RecentPathsConfigs.ConfigureDataTable()
        });
    }

    private void ReadCatalog()
    {
        assets = ReadAssets();
        folders = ReadFolders();
        syncAssetsConfiguration.Definitions.AddRange(ReadSyncAssetsDirectoriesDefinitions());
        recentTargetPaths = ReadRecentTargetPaths();

        for (int i = 0; i < assets.Count; i++)
        {
            // TODO: Improve the mapping for perf
            assets[i].Folder = GetFolderById(assets[i].FolderId)!; // If the folder is not found, that means the DB has been modified manually

            // Not saved in DB because it is computed each time to detect file update
            _storageService.UpdateAssetFileProperties(assets[i]);
        }
    }

    private List<Folder> ReadFolders()
    {
        return _database.ReadObjectList(_userConfigurationService.StorageSettings.TablesSettings.FoldersTableName, FolderConfigs.ReadFunc);
    }

    private List<Asset> ReadAssets()
    {
        return _database.ReadObjectList(_userConfigurationService.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
    }

    private List<SyncAssetsDirectoriesDefinition> ReadSyncAssetsDirectoriesDefinitions()
    {
        return _database.ReadObjectList(
            _userConfigurationService.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName,
            SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);
    }

    private List<string> ReadRecentTargetPaths()
    {
        return _database.ReadObjectList(_userConfigurationService.StorageSettings.TablesSettings.RecentTargetPathsTableName, RecentPathsConfigs.ReadFunc);
    }

    private void WriteFolders(List<Folder> foldersToWrite)
    {
        _database.WriteObjectList(foldersToWrite, _userConfigurationService.StorageSettings.TablesSettings.FoldersTableName, FolderConfigs.WriteFunc);
    }

    private void WriteAssets(List<Asset> assetsToWrite)
    {
        _database.WriteObjectList(assetsToWrite, _userConfigurationService.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.WriteFunc);
    }

    private void WriteSyncAssetsDirectoriesDefinitions(List<SyncAssetsDirectoriesDefinition> definitionsToWrite)
    {
        _database.WriteObjectList(
            definitionsToWrite,
            _userConfigurationService.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName, SyncAssetsDirectoriesDefinitionConfigs.WriteFunc);
    }

    private void WriteRecentTargetPaths(List<string> recentTargetPathsToWrite)
    {
        _database.WriteObjectList(recentTargetPathsToWrite, _userConfigurationService.StorageSettings.TablesSettings.RecentTargetPathsTableName, RecentPathsConfigs.WriteFunc);
    }

    private Dictionary<string, byte[]> GetThumbnails(Folder? folder, out bool isNewFile)
    {
        isNewFile = false;
        Dictionary<string, byte[]>? thumbnails = [];

        if (folder == null)
        {
            return thumbnails;
        }

        thumbnails = _database.ReadBlob(folder.ThumbnailsFilename); // ReadBlob returns a dict with the key as the image name and the value the byte[] (image data)

        if (thumbnails == null)
        {
            thumbnails = [];
            isNewFile = true;
        }

        return thumbnails;
    }

    private void RemoveOldThumbnailsDictionaryEntries(Folder? folder)
    {
        if (folder == null)
        {
            return;
        }

        ushort entriesToKeep = _userConfigurationService.StorageSettings.ThumbnailsDictionaryEntriesToKeep;

        if (!recentThumbnailsQueue.Contains(folder.Path))
        {
            recentThumbnailsQueue.Enqueue(folder.Path);
        }

        if (recentThumbnailsQueue.Count > entriesToKeep)
        {
            string pathToRemove = recentThumbnailsQueue.Dequeue();
            Thumbnails.Remove(pathToRemove);
        }
    }

    // One Blob per folder
    private void SaveThumbnails(Dictionary<string, byte[]> thumbnails, string thumbnailsFileName)
    {
        _database.WriteBlob(thumbnails, thumbnailsFileName);
    }

    private Folder? GetFolderById(Guid folderId)
    {
        Folder? result;

        lock (syncLock)
        {
            result = folders.FirstOrDefault(f => f.FolderId == folderId);
        }

        return result;
    }

    private List<Asset> GetAssetsByFolderId(Guid folderId)
    {
        List<Asset> result;

        lock (syncLock)
        {
            result = assets.Where(a => a.FolderId == folderId).ToList();
        }

        return result;
    }

    private Asset? GetAssetByFolderIdAndFileName(Guid folderId, string fileName)
    {
        Asset? asset;

        lock (syncLock)
        {
            asset = assets.FirstOrDefault(a => a.FolderId == folderId && a.FileName == fileName);
        }

        return asset;
    }

    #endregion
}
