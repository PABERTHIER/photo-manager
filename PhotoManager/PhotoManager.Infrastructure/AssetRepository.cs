using log4net;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;

namespace PhotoManager.Infrastructure;

public class AssetRepository : IAssetRepository
{
    private const int RECENT_TARGET_PATHS_MAX_COUNT = 20;

    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    private readonly string _dataDirectory;
    private readonly IDatabase _database;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IImageMetadataService _imageMetadataService;
    private readonly IUserConfigurationService _userConfigurationService;

    private List<Asset> _assets;
    private List<Folder> _folders;
    private SyncAssetsConfiguration _syncAssetsConfiguration;
    private List<string> _recentTargetPaths;
    protected Dictionary<string, Dictionary<string, byte[]>> Thumbnails { get; }
    private readonly Queue<string> _recentThumbnailsQueue;
    private bool IsInitialized { get; set; }
    private bool _hasChanges;
    private readonly Lock _syncLock;
    private readonly Subject<Unit> _assetsUpdatedSubject = new();
    public IObservable<Unit> AssetsUpdated => _assetsUpdatedSubject.AsObservable();

    public AssetRepository(
        IDatabase database,
        IPathProviderService pathProviderService,
        IImageProcessingService imageProcessingService,
        IImageMetadataService imageMetadataService,
        IUserConfigurationService userConfigurationService)
    {
        _database = database;
        _imageProcessingService = imageProcessingService;
        _imageMetadataService = imageMetadataService;
        _userConfigurationService = userConfigurationService;
        _assets = [];
        _folders = [];
        _syncAssetsConfiguration = new SyncAssetsConfiguration();
        _recentTargetPaths = [];
        _recentThumbnailsQueue = new Queue<string>();
        Thumbnails = [];
        _syncLock = new Lock();
        _dataDirectory = pathProviderService.ResolveDataDirectory();
        Initialize();
    }

    public Asset[] GetAssetsByPath(string directory)
    {
        List<Asset> assetsList = [];
        bool isNewFile = false;

        try
        {
            lock (_syncLock)
            {
                Folder? folder = GetFolderByPath(directory);

                if (folder != null)
                {
                    assetsList = GetAssetsByFolderId(folder.Id);

                    if (!Thumbnails.ContainsKey(folder.Path))
                    {
                        Thumbnails[folder.Path] = GetThumbnails(folder, out isNewFile);
                        RemoveOldThumbnailsDictionaryEntries(folder);
                    }

                    if (!isNewFile)
                    {
                        foreach (Asset asset in assetsList)
                        {
                            if (Thumbnails.TryGetValue(folder.Path, out Dictionary<string, byte[]>? thumbnail)
                                && thumbnail.TryGetValue(asset.FileName, out byte[]? buffer))
                            {
                                asset.ImageData = _imageProcessingService.LoadBitmapThumbnailImage(
                                    buffer,
                                    asset.Pixel.Thumbnail.Width,
                                    asset.Pixel.Thumbnail.Height);
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
            Log.Error(ex);
        }

        return [.. assetsList];
    }

    // TODO: Return created Asset
    public void AddAsset(Asset asset, byte[] thumbnailData)
    {
        lock (_syncLock)
        {
            Folder? folder = GetFolderById(asset.FolderId);

            if (string.IsNullOrWhiteSpace(asset.Folder.Path))
            {
                Log.Error($"The asset could not be added, folder path is null or empty, asset.FileName: {asset.FileName}");
                return;
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
                _assets.Add(asset);
                _hasChanges = true;
                _assetsUpdatedSubject.OnNext(Unit.Default);
            }
        }
    }

    public Folder AddFolder(string path) // Play this before anything else to register every folder
    {
        Folder folder;

        lock (_syncLock)
        {
            // TODO: To prevent side effect with duplicates for same path, need to check first GetFolderByPath(string path)
            // If not null, then we return this folder instead of adding it twice with different Id and same Path
            // Need to update some tests + add theses cases + update Application.GetRootCatalogFolders() and ApplicationVM ctor
            folder = new()
            {
                Id = Guid.NewGuid(),
                Path = path
            };

            _folders.Add(folder);
            _hasChanges = true;
        }

        return folder;
    }

    public bool FolderExists(string path)
    {
        bool result;

        lock (_syncLock)
        {
            result = _folders.Any(f => f.Path == path);
        }

        return result;
    }

    public Folder[] GetFolders()
    {
        Folder[] result;

        lock (_syncLock)
        {
            result = [.. _folders];
        }

        return result;
    }

    // TODO: Is HashSet the right thing to do ? (Because it does not preserve the order)
    public HashSet<string> GetFoldersPath()
    {
        HashSet<string> folderPaths;

        lock (_syncLock)
        {
            folderPaths = [.. _folders.Select(folder => folder.Path)];
        }

        return folderPaths;
    }

    public Folder[] GetSubFolders(Folder parentFolder)
    {
        return [.. _folders.Where(parentFolder.IsParentOf)];
    }

    public Folder? GetFolderByPath(string path)
    {
        Folder? result;

        lock (_syncLock)
        {
            result = _folders.FirstOrDefault(f => f.Path == path);
        }

        return result;
    }

    public void SaveCatalog(Folder? folder)
    {
        lock (_syncLock)
        {
            if (_hasChanges)
            {
                WriteAssets();
                WriteFolders();
                WriteSyncAssetsDirectoriesDefinitions();
                WriteRecentTargetPaths();

                _hasChanges = false;
            }

            if (folder != null && Thumbnails.TryGetValue(folder.Path, out Dictionary<string, byte[]>? thumbnail))
            {
                SaveThumbnails(thumbnail, folder.ThumbnailsFilename);
            }
        }
    }

    public bool BackupExists()
    {
        lock (_syncLock)
        {
            return _database.BackupExists(DateTime.Now.Date);
        }
    }

    public void WriteBackup()
    {
        lock (_syncLock)
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

        lock (_syncLock)
        {
            cataloguedAssets = _assets;
        }

        return cataloguedAssets;
    }

    // TODO: Improve it by having a Dict instead: Dictionary<string, List<Asset>>
    public List<Asset> GetCataloguedAssetsByPath(string directory)
    {
        List<Asset> cataloguedAssets = [];

        lock (_syncLock)
        {
            Folder? folder = GetFolderByPath(directory);

            if (folder != null)
            {
                cataloguedAssets = [.. _assets.Where(a => a.FolderId == folder.Id)];
            }
        }

        return cataloguedAssets;
    }

    public bool IsAssetCatalogued(string directoryName, string fileName)
    {
        bool result;

        lock (_syncLock)
        {
            Folder? folder = GetFolderByPath(directoryName);
            result = folder != null && GetAssetByFolderIdAndFileName(folder.Id, fileName) != null;
        }

        return result;
    }

    public Asset? DeleteAsset(string directory, string fileName)
    {
        lock (_syncLock)
        {
            Folder? folder = GetFolderByPath(directory);

            if (folder == null)
            {
                return null;
            }

            Asset? assetToDelete = GetAssetByFolderIdAndFileName(folder.Id, fileName);

            if (!Thumbnails.TryGetValue(folder.Path, out Dictionary<string, byte[]>? thumbnails))
            {
                thumbnails = GetThumbnails(folder, out _);
                Thumbnails[folder.Path] = thumbnails;
                RemoveOldThumbnailsDictionaryEntries(folder);
            }

            thumbnails.Remove(fileName);

            if (thumbnails.Count == 0)
            {
                Thumbnails.Remove(folder.Path);
                _database.DeleteBlobFile(folder.ThumbnailsFilename);
            }

            if (assetToDelete != null)
            {
                _assets.Remove(assetToDelete);
                _hasChanges = true;
                _assetsUpdatedSubject.OnNext(Unit.Default);
            }

            return assetToDelete;
        }
    }

    public void DeleteFolder(Folder folder)
    {
        lock (_syncLock)
        {
            Thumbnails.Remove(folder.Path);

            if (IsBlobFileExists(folder.ThumbnailsFilename))
            {
                _database.DeleteBlobFile(folder.ThumbnailsFilename);
            }

            _folders.Remove(folder);
            _hasChanges = true;
        }
    }

    public bool HasChanges()
    {
        bool result;

        lock (_syncLock)
        {
            result = _hasChanges;
        }

        return result;
    }

    // TODO: Seems to be a dead method
    public bool ContainsThumbnail(string directoryName, string fileName)
    {
        bool result;

        lock (_syncLock)
        {
            if (!Thumbnails.ContainsKey(directoryName))
            {
                Folder? folder = GetFolderByPath(directoryName);
                Thumbnails[directoryName] = GetThumbnails(folder, out _);
                RemoveOldThumbnailsDictionaryEntries(folder);
            }

            result = !string.IsNullOrEmpty(fileName)
                     && Thumbnails.TryGetValue(directoryName, out Dictionary<string, byte[]>? thumbnail)
                     && thumbnail.ContainsKey(fileName);
        }

        return result;
    }

    public BitmapImage? LoadThumbnail(string directoryName, string fileName, int width, int height)
    {
        BitmapImage? result = null;

        lock (_syncLock)
        {
            if (!Thumbnails.ContainsKey(directoryName))
            {
                Folder? folder = GetFolderByPath(directoryName);
                Thumbnails[directoryName] = GetThumbnails(folder, out _);
                RemoveOldThumbnailsDictionaryEntries(folder);
            }

            if (Thumbnails.TryGetValue(directoryName, out Dictionary<string, byte[]>? thumbnail)
                && thumbnail.TryGetValue(fileName, out byte[]? buffer))
            {
                result = _imageProcessingService.LoadBitmapThumbnailImage(buffer, width, height);
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

    public bool IsBlobFileExists(string blobName)
    {
        return _database.IsBlobFileExists(blobName);
    }

    public SyncAssetsConfiguration GetSyncAssetsConfiguration()
    {
        SyncAssetsConfiguration result;

        lock (_syncLock)
        {
            result = _syncAssetsConfiguration;
        }

        return result;
    }

    public void SaveSyncAssetsConfiguration(SyncAssetsConfiguration syncAssetsConfig)
    {
        lock (_syncLock)
        {
            _syncAssetsConfiguration = syncAssetsConfig;
            _hasChanges = true;
        }
    }

    public List<string> GetRecentTargetPaths()
    {
        List<string> result;

        lock (_syncLock)
        {
            result = _recentTargetPaths;
        }

        return result;
    }

    public void SaveRecentTargetPaths(List<string> paths)
    {
        lock (_syncLock)
        {
            _recentTargetPaths = paths;
            _hasChanges = true;
        }
    }

    public void UpdateTargetPathToRecent(Folder destinationFolder)
    {
        lock (_syncLock)
        {
            List<string> recentTargetPathsUpdated = [.. _recentTargetPaths];

            if (recentTargetPathsUpdated.Contains(destinationFolder.Path))
            {
                recentTargetPathsUpdated.Remove(destinationFolder.Path);
            }

            recentTargetPathsUpdated.Insert(0, destinationFolder.Path);

            recentTargetPathsUpdated = [.. recentTargetPathsUpdated.Take(RECENT_TARGET_PATHS_MAX_COUNT)];

            SaveRecentTargetPaths(recentTargetPathsUpdated);
        }
    }

    public int GetAssetsCounter()
    {
        lock (_syncLock)
        {
            return _assets.Count;
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
            _dataDirectory,
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
        _assets = ReadAssets();
        _folders = ReadFolders();
        _syncAssetsConfiguration.Definitions.AddRange(ReadSyncAssetsDirectoriesDefinitions());
        _recentTargetPaths = ReadRecentTargetPaths();

        for (int i = 0; i < _assets.Count; i++)
        {
            // TODO: Improve the mapping for perf
            _assets[i].Folder = GetFolderById(_assets[i].FolderId)!; // If the folder is not found, that means the DB has been modified manually

            // Not saved in DB because it is computed each time to detect file update
            _imageMetadataService.UpdateAssetFileProperties(_assets[i]);
        }
    }

    private List<Asset> ReadAssets()
    {
        return _database.ReadObjectList(_userConfigurationService.StorageSettings.TablesSettings.AssetsTableName,
            AssetConfigs.ReadFunc);
    }

    private List<Folder> ReadFolders()
    {
        return _database.ReadObjectList(
            _userConfigurationService.StorageSettings.TablesSettings.FoldersTableName,
            FolderConfigs.ReadFunc);
    }

    private List<SyncAssetsDirectoriesDefinition> ReadSyncAssetsDirectoriesDefinitions()
    {
        return _database.ReadObjectList(
            _userConfigurationService.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName,
            SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);
    }

    private List<string> ReadRecentTargetPaths()
    {
        return _database.ReadObjectList(
            _userConfigurationService.StorageSettings.TablesSettings.RecentTargetPathsTableName,
            RecentPathsConfigs.ReadFunc);
    }

    private void WriteAssets()
    {
        _database.WriteObjectList(
            _assets,
            _userConfigurationService.StorageSettings.TablesSettings.AssetsTableName,
            AssetConfigs.WriteFunc);
    }

    private void WriteFolders()
    {
        _database.WriteObjectList(
            _folders,
            _userConfigurationService.StorageSettings.TablesSettings.FoldersTableName,
            FolderConfigs.WriteFunc);
    }

    private void WriteSyncAssetsDirectoriesDefinitions()
    {
        _database.WriteObjectList(
            _syncAssetsConfiguration.Definitions,
            _userConfigurationService.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName,
            SyncAssetsDirectoriesDefinitionConfigs.WriteFunc);
    }

    private void WriteRecentTargetPaths()
    {
        _database.WriteObjectList(
            _recentTargetPaths,
            _userConfigurationService.StorageSettings.TablesSettings.RecentTargetPathsTableName,
            RecentPathsConfigs.WriteFunc);
    }

    private Dictionary<string, byte[]> GetThumbnails(Folder? folder, out bool isNewFile)
    {
        isNewFile = false;
        Dictionary<string, byte[]>? thumbnails = [];

        if (folder == null)
        {
            return thumbnails;
        }

        // ReadBlob returns a dict with the key as the image name and the value the byte[] (image data)
        thumbnails = _database.ReadBlob(folder.ThumbnailsFilename);

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

        if (!_recentThumbnailsQueue.Contains(folder.Path))
        {
            _recentThumbnailsQueue.Enqueue(folder.Path);
        }

        if (_recentThumbnailsQueue.Count > entriesToKeep)
        {
            string pathToRemove = _recentThumbnailsQueue.Dequeue();
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

        lock (_syncLock)
        {
            result = _folders.FirstOrDefault(f => f.Id == folderId);
        }

        return result;
    }

    private List<Asset> GetAssetsByFolderId(Guid folderId)
    {
        List<Asset> result;

        lock (_syncLock)
        {
            result = [.. _assets.Where(a => a.FolderId == folderId)];
        }

        return result;
    }

    private Asset? GetAssetByFolderIdAndFileName(Guid folderId, string fileName)
    {
        Asset? asset;

        lock (_syncLock)
        {
            asset = _assets.FirstOrDefault(a => a.FolderId == folderId && a.FileName == fileName);
        }

        return asset;
    }

    #endregion
}
