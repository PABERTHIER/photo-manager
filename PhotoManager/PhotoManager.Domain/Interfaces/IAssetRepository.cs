﻿using System.Reactive;

namespace PhotoManager.Domain.Interfaces;

public interface IAssetRepository
{
    IObservable<Unit> AssetsUpdated { get; }
    Asset[] GetAssetsByPath(string directory);
    void AddAsset(Asset asset, byte[] thumbnailData);
    Folder AddFolder(string path);
    bool FolderExists(string path);
    Folder[] GetFolders();
    HashSet<string> GetFoldersPath();
    Folder[] GetSubFolders(Folder parentFolder);
    Folder? GetFolderByPath(string path);
    void SaveCatalog(Folder? folder);
    bool BackupExists();
    void WriteBackup();
    List<Asset> GetCataloguedAssets();
    List<Asset> GetCataloguedAssetsByPath(string directory);
    bool IsAssetCatalogued(string directoryName, string fileName);
    Asset? DeleteAsset(string directory, string deletedFileName);
    void DeleteFolder(Folder folder);
    bool HasChanges();
    bool ContainsThumbnail(string directoryName, string fileName);
    BitmapImage? LoadThumbnail(string directoryName, string fileName, int width, int height);
    bool FolderHasThumbnails(Folder folder);
    SyncAssetsConfiguration GetSyncAssetsConfiguration();
    void SaveSyncAssetsConfiguration(SyncAssetsConfiguration syncAssetsConfiguration);
    List<string> GetRecentTargetPaths();
    void SaveRecentTargetPaths(List<string> recentTargetPaths);
    void UpdateTargetPathToRecent(Folder destinationFolder);
    int GetAssetsCounter();
}
