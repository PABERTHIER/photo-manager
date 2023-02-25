using log4net;
using PhotoManager.Domain;
using PhotoManager.Domain.Interfaces;
using SimplePortableDatabase;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace PhotoManager.Infrastructure;

public class AssetRepository : IAssetRepository
{
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private const double STORAGE_VERSION = 1.0;
    private const string SEPARATOR = "|";

    public bool IsInitialized { get; private set; }
    private string dataDirectory;
    private readonly IDatabase _database;
    private readonly IStorageService _storageService;
    private readonly IUserConfigurationService _userConfigurationService;

    private List<Asset> assets;
    private List<VideoAsset> videoAssets;
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

    public Asset[] GetAssets(string directory)
    {
        List<Asset> assetsList = null;
        bool isNewFile = false;

        try
        {
            lock (syncLock)
            {
                Folder folder = GetFolderByPath(directory);

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
                            if (Thumbnails[folder.Path].ContainsKey(asset.FileName))
                            {
                                asset.ImageData = _storageService.LoadBitmapImage(Thumbnails[folder.Path][asset.FileName], asset.ThumbnailPixelWidth, asset.ThumbnailPixelHeight);
                            }
                        }

                        // Removes assets with no thumbnails.
                        List<Asset> assetsToRemove = new();

                        for (int i = 0; i < assetsList.Count; i++)
                        {
                            if (assetsList[i].ImageData == null)
                            {
                                assetsToRemove.Add(assetsList[i]);
                            }
                        }

                        foreach (Asset asset in assetsToRemove)
                        {
                            assetsList.Remove(asset);
                        }
                    }

                    foreach (Asset asset in assetsList)
                    {
                        _storageService.GetFileInformation(asset);
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
            Folder folder = GetFolderById(asset.FolderId);

            if (folder == null)
            {
                AddFolder(asset.Folder.Path);
            }

            if (!Thumbnails.ContainsKey(asset.Folder.Path))
            {
                Thumbnails[asset.Folder.Path] = GetThumbnails(asset.Folder, out bool isNewFile);
                RemoveOldThumbnailsDictionaryEntries(asset.Folder);
            }

            Thumbnails[asset.Folder.Path][asset.FileName] = thumbnailData;
            assets.Add(asset);
            hasChanges = true;
        }
    }

    public void AddVideoAsset(VideoAsset videoAsset, byte[] thumbnailData)
    {
        lock (syncLock)
        {
            Folder folder = GetFolderById(videoAsset.FolderId);

            if (folder == null)
            {
                AddFolder(videoAsset.Folder.Path);
            }

            if (!Thumbnails.ContainsKey(videoAsset.Folder.Path))
            {
                Thumbnails[videoAsset.Folder.Path] = GetThumbnails(videoAsset.Folder, out bool isNewFile);
                RemoveOldThumbnailsDictionaryEntries(videoAsset.Folder);
            }

            Thumbnails[videoAsset.Folder.Path][videoAsset.FileName] = thumbnailData;
            videoAssets.Add(videoAsset);
            hasChanges = true;
        }
    }

    public Folder AddFolder(string path)
    {
        Folder folder;

        lock (syncLock)
        {
            string folderId = Guid.NewGuid().ToString();

            folder = new Folder
            {
                FolderId = folderId,
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

    public Folder[] GetSubFolders(Folder parentFolder, bool includeHidden)
    {
        Folder[] folders = GetFolders();
        folders = folders.Where(f => parentFolder.IsParentOf(f)).ToArray();
        return folders;
    }

    public Folder GetFolderByPath(string path)
    {
        Folder result = null;

        lock (syncLock)
        {
            result = folders.FirstOrDefault(f => f.Path == path);
        }

        return result;
    }

    public void SaveCatalog(Folder folder)
    {
        lock (syncLock)
        {
            if (hasChanges)
            {
                WriteAssets(assets);
                WriteFolders(folders);
                WriteSyncDefinitions(_syncAssetsConfiguration.Definitions);
                WriteRecentTargetPaths(_recentTargetPaths);
            }

            hasChanges = false;

            if (Thumbnails != null && folder != null && Thumbnails.ContainsKey(folder.Path))
            {
                SaveThumbnails(Thumbnails[folder.Path], folder.ThumbnailsFilename);
            }
        }
    }

    public bool BackupExists()
    {
        return _database.BackupExists(DateTime.Now.Date);
    }

    public void WriteBackup()
    {
        if (_database.WriteBackup(DateTime.Now.Date)) // TODO: System.IO.IOException: 'The process cannot access the file X because it is being used by another process.'
        {
            _database.DeleteOldBackups(_userConfigurationService.GetBackupsToKeep());
        }
    }

    public List<Asset> GetCataloguedAssets()
    {
        List<Asset> cataloguedAssets = null;

        lock (syncLock)
        {
            cataloguedAssets = assets;
        }

        return cataloguedAssets;
    }

    public List<Asset> GetCataloguedAssets(string directory)
    {
        List<Asset> cataloguedAssets = null;

        lock (syncLock)
        {
            Folder folder = GetFolderByPath(directory);

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
            Folder folder = GetFolderByPath(directoryName);
            result = folder != null && GetAssetByFolderIdFileName(folder.FolderId, fileName) != null;
        }

        return result;
    }

    public void DeleteAsset(string directory, string fileName)
    {
        lock (syncLock)
        {
            Folder folder = GetFolderByPath(directory);

            if (folder != null)
            {
                Asset deletedAsset = GetAssetByFolderIdFileName(folder.FolderId, fileName);

                if (!Thumbnails.ContainsKey(folder.Path))
                {
                    try
                    {
                        Thumbnails[folder.Path] = GetThumbnails(folder, out bool isNewFile);
                        RemoveOldThumbnailsDictionaryEntries(folder);
                    }
                    catch
                    {
                    }

                }

                if (Thumbnails.ContainsKey(folder.Path))
                {
                    Thumbnails[folder.Path].Remove(fileName);
                }

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
            if (folder != null)
            {
                if (Thumbnails.ContainsKey(folder.Path))
                {
                    Thumbnails.Remove(folder.Path);
                }

                if (FolderHasThumbnails(folder))
                {
                    DeleteThumbnails(folder);
                }

                folders.Remove(folder);
                hasChanges = true;
            }
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
                Folder folder = GetFolderByPath(directoryName);
                Thumbnails[directoryName] = GetThumbnails(folder, out bool isNewFile);
                RemoveOldThumbnailsDictionaryEntries(folder);
            }

            result = !string.IsNullOrEmpty(fileName) && Thumbnails[directoryName].ContainsKey(fileName);
        }

        return result;
    }

    public BitmapImage LoadThumbnail(string directoryName, string fileName, int width, int height)
    {
        BitmapImage result = null;

        lock (syncLock)
        {
            if (!Thumbnails.ContainsKey(directoryName))
            {
                Folder folder = GetFolderByPath(directoryName);
                Thumbnails[directoryName] = GetThumbnails(folder, out bool isNewFile);
                RemoveOldThumbnailsDictionaryEntries(folder);
            }

            if (Thumbnails[directoryName].ContainsKey(fileName))
            {
                result = _storageService.LoadBitmapImage(Thumbnails[directoryName][fileName], width, height);
            }
            else
            {
                DeleteAsset(directoryName, fileName);
                Folder folder = GetFolderByPath(directoryName);
                SaveCatalog(folder);
            }
        }

        return result;
    }

    public bool FolderHasThumbnails(Folder folder)
    {
        string thumbnailsFilePath = _database.ResolveBlobFilePath(dataDirectory, folder.ThumbnailsFilename);
        // TODO: Implement through the NuGet package.
        return File.Exists(thumbnailsFilePath);
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
        List<string> result = null;

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

    public int AssetsCounter()
    {
        return assets.Count;
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
        dataDirectory = _storageService.ResolveDataDirectory(STORAGE_VERSION);
        var separatorChar = SEPARATOR.ToCharArray().First();
        _database.Initialize(dataDirectory, separatorChar);

        _database.SetDataTableProperties(new DataTableProperties
        {
            TableName = "Folder",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "FolderId" },
                new ColumnProperties { ColumnName = "Path" }
            }
        });

        _database.SetDataTableProperties(new DataTableProperties
        {
            TableName = "Asset",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "FolderId" },
                new ColumnProperties { ColumnName = "FileName" },
                new ColumnProperties { ColumnName = "FileSize" },
                new ColumnProperties { ColumnName = "ImageRotation" },
                new ColumnProperties { ColumnName = "PixelWidth" },
                new ColumnProperties { ColumnName = "PixelHeight" },
                new ColumnProperties { ColumnName = "ThumbnailPixelWidth" },
                new ColumnProperties { ColumnName = "ThumbnailPixelHeight" },
                new ColumnProperties { ColumnName = "ThumbnailCreationDateTime" },
                new ColumnProperties { ColumnName = "Hash" },
                new ColumnProperties { ColumnName = "AssetCorruptedMessage" },
                new ColumnProperties { ColumnName = "IsAssetCorrupted" },
                new ColumnProperties { ColumnName = "AssetRotatedMessage" },
                new ColumnProperties { ColumnName = "IsAssetRotated" }
            }
        });

        _database.SetDataTableProperties(new DataTableProperties
        {
            TableName = "Import",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "SourceDirectory" },
                new ColumnProperties { ColumnName = "DestinationDirectory" },
                new ColumnProperties { ColumnName = "IncludeSubFolders" },
                new ColumnProperties { ColumnName = "DeleteAssetsNotInSource" }
            }
        });

        _database.SetDataTableProperties(new DataTableProperties
        {
            TableName = "RecentTargetPaths",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "Path" }
            }
        });
    }

    private void ReadCatalog()
    {
        assets = ReadAssets();
        folders = ReadFolders();
        _syncAssetsConfiguration = new SyncAssetsConfiguration();
        _syncAssetsConfiguration.Definitions.AddRange(ReadSyncDefinitions());
        assets.ForEach(a => a.Folder = GetFolderById(a.FolderId));
        _recentTargetPaths = ReadRecentTargetPaths();
    }

    private List<Folder> ReadFolders()
    {
        List<Folder> result;

        try
        {
            result = _database.ReadObjectList("Folder", f =>
                new Folder
                {
                    FolderId = f[0],
                    Path = f[1]
                });
        }
        catch (ArgumentException ex)
        {
            throw new ApplicationException($"Error while trying to read data table 'Folder'. " +
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
            result = _database.ReadObjectList("Asset", f =>
                new Asset
                {
                    FolderId = f[0],
                    FileName = f[1],
                    FileSize = long.Parse(f[2]),
                    ImageRotation = (Rotation)Enum.Parse(typeof(Rotation), f[3]),
                    PixelWidth = int.Parse(f[4]),
                    PixelHeight = int.Parse(f[5]),
                    ThumbnailPixelWidth = int.Parse(f[6]),
                    ThumbnailPixelHeight = int.Parse(f[7]),
                    ThumbnailCreationDateTime = DateTime.Parse(f[8]),
                    Hash = f[9],
                    AssetCorruptedMessage = f[10],
                    IsAssetCorrupted = bool.Parse(f[11]),
                    AssetRotatedMessage = f[12],
                    IsAssetRotated = bool.Parse(f[13])
                });
        }
        catch (ArgumentException ex)
        {
            throw new ApplicationException($"Error while trying to read data table 'Asset'. " +
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
            result = _database.ReadObjectList("Import", f =>
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = f[0],
                    DestinationDirectory = f[1],
                    IncludeSubFolders = bool.Parse(f[2]),
                    DeleteAssetsNotInSource = f.Length > 3 && bool.Parse(f[3])
                });
        }
        catch (ArgumentException ex)
        {
            throw new ApplicationException($"Error while trying to read data table 'Import'. " +
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
            result = _database.ReadObjectList("RecentTargetPaths", f => f[0]);
        }
        catch (ArgumentException ex)
        {
            throw new ApplicationException($"Error while trying to read data table 'RecentTargetPaths'. " +
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
        _database.WriteObjectList(folders, "Folder", (f, i) =>
        {
            return i switch
            {
                0 => f.FolderId,
                1 => f.Path,
                _ => throw new ArgumentOutOfRangeException(nameof(i))
            };
        });
    }

    private void WriteAssets(List<Asset> assets)
    {
        _database.WriteObjectList(assets, "Asset", (a, i) =>
        {
            return i switch
            {
                0 => a.FolderId,
                1 => a.FileName,
                2 => a.FileSize,
                3 => a.ImageRotation,
                4 => a.PixelWidth,
                5 => a.PixelHeight,
                6 => a.ThumbnailPixelWidth,
                7 => a.ThumbnailPixelHeight,
                8 => a.ThumbnailCreationDateTime,
                9 => a.Hash,
                10 => a.AssetCorruptedMessage,
                11 => a.IsAssetCorrupted,
                12 => a.AssetRotatedMessage,
                13 => a.IsAssetRotated,
                _ => throw new ArgumentOutOfRangeException(nameof(i))
            };
        });
    }

    private void WriteSyncDefinitions(List<SyncAssetsDirectoriesDefinition> definitions)
    {
        _database.WriteObjectList(definitions, "Import", (d, i) =>
        {
            return i switch
            {
                0 => d.SourceDirectory,
                1 => d.DestinationDirectory,
                2 => d.IncludeSubFolders,
                3 => d.DeleteAssetsNotInSource,
                _ => throw new ArgumentOutOfRangeException(nameof(i))
            };
        });
    }

    private void WriteRecentTargetPaths(List<string> recentTargetPaths)
    {
        _database.WriteObjectList(recentTargetPaths, "RecentTargetPaths", (p, i) =>
        {
            return i switch
            {
                0 => p,
                _ => throw new ArgumentOutOfRangeException(nameof(i))
            };
        });
    }

    private void DeleteThumbnails(Folder folder)
    {
        // TODO: Implement through the NuGet package.
        string thumbnailsFilePath = _database.ResolveBlobFilePath(dataDirectory, folder.ThumbnailsFilename);
        File.Delete(thumbnailsFilePath);
    }

    protected virtual Dictionary<string, byte[]> GetThumbnails(Folder folder, out bool isNewFile)
    {
        isNewFile = false;
        Dictionary<string, byte[]> thumbnails = (Dictionary<string, byte[]>)_database.ReadBlob(folder.ThumbnailsFilename);

        if (thumbnails == null)
        {
            thumbnails = new Dictionary<string, byte[]>();
            isNewFile = true;
        }

        return thumbnails;
    }

    private void RemoveOldThumbnailsDictionaryEntries(Folder folder)
    {
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

    private void SaveThumbnails(Dictionary<string, byte[]> thumbnails, string thumbnailsFileName)
    {
        _database.WriteBlob(thumbnails, thumbnailsFileName);
    }

    private Folder GetFolderById(string folderId)
    {
        Folder result = null;

        lock (syncLock)
        {
            result = folders.FirstOrDefault(f => f.FolderId == folderId);
        }

        return result;
    }

    private List<Asset> GetAssetsByFolderId(string folderId)
    {
        List<Asset> result = null;

        lock (syncLock)
        {
            result = assets.Where(a => a.FolderId == folderId).ToList();
        }

        return result;
    }

    private Asset GetAssetByFolderIdFileName(string folderId, string fileName)
    {
        Asset result = null;

        lock (syncLock)
        {
            result = assets.FirstOrDefault(a => a.FolderId == folderId && a.FileName == fileName);
        }

        return result;
    }

    #endregion
}
