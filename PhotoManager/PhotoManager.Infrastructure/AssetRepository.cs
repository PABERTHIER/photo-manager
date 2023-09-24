using log4net;
using System.Reflection;

namespace PhotoManager.Infrastructure;

public class AssetRepository : IAssetRepository
{
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public bool IsInitialized { get; private set; }
    private string dataDirectory;
    private readonly IDatabase _database;
    private readonly IStorageService _storageService;
    private readonly IUserConfigurationService _userConfigurationService;

    private List<Asset> assets;
    private List<Folder> folders;
    private SyncAssetsConfiguration _syncAssetsConfiguration;
    private List<string> _recentTargetPaths;
    protected Dictionary<string, Dictionary<string, byte[]>> Thumbnails { get; private set; }
    private readonly Queue<string> recentThumbnailsQueue;
    private bool hasChanges;
    private readonly object syncLock;

    public AssetRepository(IDatabase database, IStorageService storageService, IUserConfigurationService userConfigurationService)
    {
        _database = database;
        _storageService = storageService;
        _userConfigurationService = userConfigurationService;
        Thumbnails = new Dictionary<string, Dictionary<string, byte[]>>();
        recentThumbnailsQueue = new Queue<string>();
        syncLock = new object();
        Initialize();
    }

    public Asset[] GetAssetsByPath(string directory)
    {
        List<Asset> assetsList = new (); // TODO: Why array at the end ?
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
                                asset.ImageData = _storageService.LoadBitmapThumbnailImage(thumbnail[asset.FileName], asset.ThumbnailPixelWidth, asset.ThumbnailPixelHeight);
                            }
                        }

                        // Removes assets with no thumbnails
                        assetsList.RemoveAll(asset => asset.ImageData == null);
                    }

                    foreach (Asset asset in assetsList)
                    {
                        _storageService.LoadFileInformation(asset);
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

    public void AddAsset(Asset asset, byte[] thumbnailData)
    {
        lock (syncLock)
        {
            Folder? folder = GetFolderById(asset.FolderId);

            if (string.IsNullOrWhiteSpace(asset.Folder?.Path))
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

            if (Thumbnails.TryGetValue(asset.Folder.Path, out var folderThumbnails))
            {
                folderThumbnails[asset.FileName] = thumbnailData;
                assets.Add(asset);
                hasChanges = true;
            }

        }
    }

    public Folder AddFolder(string path) // Play this before anything else to register every folders
    {
        Folder folder;

        lock (syncLock)
        {
            folder = new Folder
            {
                FolderId = Guid.NewGuid().ToString(), // TODO: Why not a Guid? ?
                Path = path
            };

            folders.Add(folder);
            hasChanges = true;
        }

        return folder;
    }

    public bool FolderExists(string path)
    {
        bool result = false;

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
            result = folders.ToArray();
        }

        return result;
    }

    public Folder[] GetSubFolders(Folder parentFolder, bool includeHidden) // TODO: Remove includeHidden when tests done in the caller class
    {
        return folders.Where(parentFolder.IsParentOf).ToArray();
    }

    public Folder? GetFolderByPath(string path)
    {
        Folder? result = null;

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
                WriteSyncDefinitions(_syncAssetsConfiguration.Definitions);
                WriteRecentTargetPaths(_recentTargetPaths);

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
                _database.DeleteOldBackups(_userConfigurationService.GetBackupsToKeep());
            }
        }
    }

    public List<Asset> GetCataloguedAssets()
    {
        List<Asset>? cataloguedAssets = null;

        lock (syncLock)
        {
            cataloguedAssets = assets;
        }

        return cataloguedAssets;
    }

    public List<Asset> GetCataloguedAssetsByPath(string directory)
    {
        List<Asset> cataloguedAssets = new ();

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
        bool result = false;

        lock (syncLock)
        {
            Folder? folder = GetFolderByPath(directoryName);
            result = folder != null && GetAssetByFolderIdAndFileName(folder.FolderId, fileName) != null;
        }

        return result;
    }

    public void DeleteAsset(string directory, string fileName)
    {
        lock (syncLock)
        {
            Folder? folder = GetFolderByPath(directory);

            if (folder != null)
            {
                Asset? deletedAsset = GetAssetByFolderIdAndFileName(folder.FolderId, fileName);

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

                if (deletedAsset != null)
                {
                    assets.Remove(deletedAsset);
                    hasChanges = true;
                }
            }
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
        bool result = false;

        lock (syncLock)
        {
            result = hasChanges;
        }

        return result;
    }

    public bool ContainsThumbnail(string directoryName, string fileName)
    {
        bool result = false;

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
                DeleteAsset(directoryName, fileName);
                Folder? folder = GetFolderByPath(directoryName);
                SaveCatalog(folder);
            }
        }

        return result;
    }

    public bool FolderHasThumbnails(Folder folder)
    {
        return _database.FolderHasThumbnails(folder.ThumbnailsFilename);
    }

    public SyncAssetsConfiguration GetSyncAssetsConfiguration()
    {
        SyncAssetsConfiguration result;

        lock (syncLock)
        {
            result = _syncAssetsConfiguration;
        }

        return result;
    }

    public void SaveSyncAssetsConfiguration(SyncAssetsConfiguration syncAssetsConfiguration)
    {
        lock (syncLock)
        {
            _syncAssetsConfiguration = syncAssetsConfiguration;
            hasChanges = true;
        }
    }

    public List<string> GetRecentTargetPaths()
    {
        List<string> result = new ();

        lock (syncLock)
        {
            result = _recentTargetPaths;
        }

        return result;
    }

    public void SaveRecentTargetPaths(List<string> recentTargetPaths)
    {
        lock (syncLock)
        {
            _recentTargetPaths = recentTargetPaths;
            hasChanges = true;
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

            if (assets == null)
            {
                SaveCatalog(null);
            }

            IsInitialized = true;
        }
    }

    private void InitializeDatabase()
    {
        dataDirectory = _storageService.ResolveDataDirectory(AssetConstants.StorageVersion);
        var separatorChar = AssetConstants.Separator.ToCharArray().First();
        _database.Initialize(dataDirectory, separatorChar);

        _database.SetDataTableProperties(new DataTableProperties
        {
            TableName = AssetConstants.FolderTableName,
            ColumnProperties = FolderConfigs.ConfigureDataTable()
        });

        _database.SetDataTableProperties(new DataTableProperties
        {
            TableName = AssetConstants.AssetTableName,
            ColumnProperties = AssetConfigs.ConfigureDataTable()
        });

        _database.SetDataTableProperties(new DataTableProperties
        {
            TableName = AssetConstants.ImportTableName,
            ColumnProperties = SyncDefinitionConfigs.ConfigureDataTable()
        });

        _database.SetDataTableProperties(new DataTableProperties
        {
            TableName = AssetConstants.RecentTargetPathsTableName,
            ColumnProperties = RecentPathsConfigs.ConfigureDataTable()
        });
    }

    private void ReadCatalog()
    {
        assets = ReadAssets();
        folders = ReadFolders();
        _syncAssetsConfiguration = new SyncAssetsConfiguration();
        _syncAssetsConfiguration.Definitions.AddRange(ReadSyncDefinitions());
        assets.ForEach(a => a.Folder = GetFolderById(a.FolderId)); // TODO: fix this
        _recentTargetPaths = ReadRecentTargetPaths();
    }

    private List<Folder> ReadFolders()
    {
        List<Folder> result;

        try
        {
            result = _database.ReadObjectList(AssetConstants.FolderTableName, FolderConfigs.ReadFunc);
        }
        catch (ArgumentException ex)
        {
            throw new ApplicationException("Error while trying to read data table 'Folder'. " +
                $"DataDirectory: {_database.DataDirectory} - " +
                $"Separator: {_database.Separator} - " +
                $"LastReadFilePath: {_database.Diagnostics.LastReadFilePath} - " +
                $"LastReadFileRaw: {_database.Diagnostics.LastReadFileRaw}",
                ex);
        }

        return result;
    }

    private List<Asset> ReadAssets()
    {
        List<Asset> result;

        try
        {
            result = _database.ReadObjectList(AssetConstants.AssetTableName, AssetConfigs.ReadFunc);
        }
        catch (ArgumentException ex)
        {
            throw new ApplicationException("Error while trying to read data table 'Asset'. " +
                $"DataDirectory: {_database.DataDirectory} - " +
                $"Separator: {_database.Separator} - " +
                $"LastReadFilePath: {_database.Diagnostics.LastReadFilePath} - " +
                $"LastReadFileRaw: {_database.Diagnostics.LastReadFileRaw}",
                ex);
        }

        return result;
    }

    private List<SyncAssetsDirectoriesDefinition> ReadSyncDefinitions()
    {
        List<SyncAssetsDirectoriesDefinition> result;

        try
        {
            result = _database.ReadObjectList(AssetConstants.ImportTableName, SyncDefinitionConfigs.ReadFunc);
        }
        catch (ArgumentException ex)
        {
            throw new ApplicationException("Error while trying to read data table 'Import'. " +
                $"DataDirectory: {_database.DataDirectory} - " +
                $"Separator: {_database.Separator} - " +
                $"LastReadFilePath: {_database.Diagnostics.LastReadFilePath} - " +
                $"LastReadFileRaw: {_database.Diagnostics.LastReadFileRaw}",
                ex);
        }

        return result;
    }

    private List<string> ReadRecentTargetPaths()
    {
        List<string> result;

        try
        {
            result = _database.ReadObjectList(AssetConstants.RecentTargetPathsTableName, RecentPathsConfigs.ReadFunc);
        }
        catch (ArgumentException ex)
        {
            throw new ApplicationException("Error while trying to read data table 'RecentTargetPaths'. " +
                $"DataDirectory: {_database.DataDirectory} - " +
                $"Separator: {_database.Separator} - " +
                $"LastReadFilePath: {_database.Diagnostics.LastReadFilePath} - " +
                $"LastReadFileRaw: {_database.Diagnostics.LastReadFileRaw}",
                ex);
        }

        return result;
    }

    private void WriteFolders(List<Folder> folders)
    {
        _database.WriteObjectList(folders, AssetConstants.FolderTableName, FolderConfigs.WriteFunc);
    }

    private void WriteAssets(List<Asset> assets)
    {
        _database.WriteObjectList(assets, AssetConstants.AssetTableName, AssetConfigs.WriteFunc);
    }

    private void WriteSyncDefinitions(List<SyncAssetsDirectoriesDefinition> definitions)
    {
        _database.WriteObjectList(definitions, AssetConstants.ImportTableName, SyncDefinitionConfigs.WriteFunc);
    }

    private void WriteRecentTargetPaths(List<string> recentTargetPaths)
    {
        _database.WriteObjectList(recentTargetPaths, AssetConstants.RecentTargetPathsTableName, RecentPathsConfigs.WriteFunc);
    }

    private Dictionary<string, byte[]> GetThumbnails(Folder? folder, out bool isNewFile)
    {
        isNewFile = false;
        Dictionary<string, byte[]>? thumbnails = new ();

        if (folder == null)
        {
            return thumbnails;
        }

        thumbnails = _database.ReadBlob(folder.ThumbnailsFilename); // ReadBlob returns a dict with the key as the image name and the value the byte[] (image data)

        if (thumbnails == null)
        {
            thumbnails = new Dictionary<string, byte[]>();
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

        int entriesToKeep = _userConfigurationService.GetThumbnailsDictionaryEntriesToKeep();

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

    private Folder? GetFolderById(string folderId) // Why not a Guid? ?
    {
        Folder? result = null;

        lock (syncLock)
        {
            result = folders.FirstOrDefault(f => f.FolderId == folderId);
        }

        return result;
    }

    private List<Asset> GetAssetsByFolderId(string folderId)  // Why not a Guid? ?
    {
        List<Asset> result = new ();

        lock (syncLock)
        {
            result = assets.Where(a => a.FolderId == folderId).ToList();
        }

        return result;
    }

    private Asset? GetAssetByFolderIdAndFileName(string folderId, string fileName) // Why not a Guid? ?
    {
        Asset? asset = null;

        lock (syncLock)
        {
            asset = assets.FirstOrDefault(a => a.FolderId == folderId && a.FileName == fileName);
        }

        return asset;
    }

    #endregion
}
