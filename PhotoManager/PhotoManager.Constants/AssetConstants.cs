namespace PhotoManager.Constants;

public static class AssetConstants
{
    public const bool UsingMD5Hash = false; // Enable it to Hash in MD5
    // Performances are decreased with PHash by 6 times (for ex: 0.17s for 140 pictures with SHA512 and 1.11s with PHash)
    public const bool UsingPHash = false; // Enable it to detect duplicates between rotated assets (improve detection) PHash = Perceptual Hash
    public const bool UsingDHash = false; // Enable it to Hash in DHash (Difference Hash)
    public const bool DetectThumbnails = false; // Enable it to detect duplicates between a thumbnail and the original
    public const bool AnalyseVideos = false; // Enable it to extract thumbnail from each videos
    public const bool SyncAssetsEveryXMinutes = false; // Enable it to sync your assets every X minutes (CATALOG_COOLDOWN_MINUTES)
    public const ushort DefaultExifOrientation = 1;
    public const ushort OrientationCorruptedImage = 10000;
    public const double MaxWidth = 200;
    public const double MaxHeight = 150;
    public const string AssetCorruptedMessage = "The asset is corrupted";
    public const string AssetRotatedMessage = "The asset has been rotated";
    public const double StorageVersion = 1.0;
    public const string Separator = "|";
    public const string AssetsTableName = "Assets";
    public const string FoldersTableName = "Folders";
    public const string SyncAssetsDirectoriesDefinitionsTableName = "SyncAssetsDirectoriesDefinitions";
    public const string RecentTargetPathsTableName = "RecentTargetPaths";
    public const string Tables = "Tables";
    public const string Blobs = "Blobs";
}
