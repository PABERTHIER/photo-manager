using System.Collections.Concurrent;
using System.Globalization;

namespace PhotoManager.Infrastructure;

// TODO: Rename to FileService
public class StorageService(IUserConfigurationService userConfigurationService) : IStorageService
{
    public List<DirectoryInfo> GetSubDirectories(string directoryPath)
    {
        return new DirectoryInfo(directoryPath).EnumerateDirectories().ToList();
    }

    public List<DirectoryInfo> GetRecursiveSubDirectories(string directoryPath)
    {
        List<DirectoryInfo> result = new();
        GetRecursiveSubDirectories(directoryPath, result);

        return result;
    }

    public string ResolveDataDirectory(double storageVersion)
    {
        return Path.Combine(userConfigurationService.PathSettings.BackupPath, "v" + storageVersion.ToString("0.0", CultureInfo.InvariantCulture));
    }

    public void CreateDirectory(string directory)
    {
        Directory.CreateDirectory(directory);
    }

    public void DeleteFile(string directory, string fileName)
    {
        string fullPath = Path.Combine(directory, fileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    public string[] GetFileNames(string directory)
    {
        string[] files = Directory.GetFiles(directory);
        return files.Select(f => Path.GetFileName(f)).ToArray();
    }

    public byte[] GetFileBytes(string filePath)
    {
        return File.ReadAllBytes(filePath);
    }

    // From CatalogAssetsService for CreateAsset() to get the thumbnailImage
    public BitmapImage LoadBitmapThumbnailImage(byte[] buffer, Rotation rotation, int width, int height)
    {
        return BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, width, height);
    }

    // From AssetRepository
    public BitmapImage LoadBitmapThumbnailImage(byte[] buffer, int width, int height)
    {
        return BitmapHelper.LoadBitmapThumbnailImage(buffer, width, height);
    }

    // From CatalogAssetsService for CreateAsset() to get the originalImage
    public BitmapImage LoadBitmapOriginalImage(byte[] buffer, Rotation rotation)
    {
        return BitmapHelper.LoadBitmapOriginalImage(buffer, rotation);
    }

    // From ShowImage() in ViewerUserControl to open the image in fullscreen mode
    public BitmapImage LoadBitmapImageFromPath(string imagePath, Rotation rotation)
    {
        return BitmapHelper.LoadBitmapImageFromPath(imagePath, rotation);
    }

    // From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC
    public BitmapImage LoadBitmapHeicOriginalImage(byte[] imageBytes, Rotation rotation)
    {
        return BitmapHelper.LoadBitmapHeicOriginalImage(imageBytes, rotation);
    }

    // From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC
    public BitmapImage LoadBitmapHeicThumbnailImage(byte[] buffer, Rotation rotation, int width, int height)
    {
        return BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height);
    }

    // From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic
    public BitmapImage LoadBitmapHeicImageFromPath(string imagePath, Rotation rotation)
    {
        return BitmapHelper.LoadBitmapHeicImageFromPath(imagePath, rotation);
    }

    public ushort GetExifOrientation(byte[] buffer, ushort defaultExifOrientation, ushort corruptedImageOrientation)
    {
        return ExifHelper.GetExifOrientation(buffer, defaultExifOrientation, corruptedImageOrientation);
    }

    public ushort GetHeicExifOrientation(byte[] buffer, ushort corruptedImageOrientation)
    {
        return ExifHelper.GetHeicExifOrientation(buffer, corruptedImageOrientation);
    }

    public Rotation GetImageRotation(ushort exifOrientation)
    {
        return ExifHelper.GetImageRotation(exifOrientation);
    }

    public byte[] GetJpegBitmapImage(BitmapImage thumbnailImage)
    {
        return BitmapHelper.GetJpegBitmapImage(thumbnailImage);
    }

    public byte[] GetPngBitmapImage(BitmapImage thumbnailImage)
    {
        return BitmapHelper.GetPngBitmapImage(thumbnailImage);
    }

    public byte[] GetGifBitmapImage(BitmapImage thumbnailImage)
    {
        return BitmapHelper.GetGifBitmapImage(thumbnailImage);
    }

    public bool FileExists(Folder folder, Asset asset)
    {
        string fullPath = Path.Combine(folder.Path, asset.FileName);
        return FileExists(fullPath);
    }

    public bool FileExists(string fullPath)
    {
        return File.Exists(fullPath);
    }

    public bool FolderExists(string fullPath)
    {
        return Directory.Exists(fullPath);
    }

    // TODO: Rename to UpdateAssetsFileDateTimeProperties
    public void LoadFilesInformation(List<Asset> assets)
    {
        for (int i = 0; i < assets.Count; i++)
        {
            LoadFileInformation(assets[i]);
        }
    }

    // TODO: Rename to UpdateAssetFileDateTimeProperties
    public void LoadFileInformation(Asset asset)
    {
        if (!FileExists(asset.FullPath))
        {
            return;
        }

        FileInfo info = new (asset.FullPath);
        asset.FileCreationDateTime = info.CreationTime;
        asset.FileModificationDateTime = info.LastWriteTime;
    }

    public bool IsValidGDIPlusImage(byte[] imageData)
    {
        return ExifHelper.IsValidGDIPlusImage(imageData);
    }

    public bool IsValidHeic(byte[] imageData)
    {
        return ExifHelper.IsValidHeic(imageData);
    }

    public int GetTotalFilesCount()
    {
        string pathSettingsAssetsDirectory = userConfigurationService.PathSettings.AssetsDirectory;
        ConcurrentDictionary<string, int> directoryFileCounts = new();

        int rootFileCount = CountFilesInDirectory(pathSettingsAssetsDirectory, SearchOption.TopDirectoryOnly);
        directoryFileCounts[pathSettingsAssetsDirectory] = rootFileCount;

        IEnumerable<string> subdirectories = Directory.EnumerateDirectories(pathSettingsAssetsDirectory);
        Parallel.ForEach(subdirectories, subdirectory =>
        {
            int fileCount = CountFilesInDirectory(subdirectory, SearchOption.AllDirectories);
            directoryFileCounts[subdirectory] = fileCount;
        });

        return directoryFileCounts.Sum(entry => entry.Value);
    }

    private static int CountFilesInDirectory(string? directoryPath, SearchOption searchOption)
    {
        return directoryPath == null ? 0 : Directory.EnumerateFiles(directoryPath, "*", searchOption).Count();
    }

    private void GetRecursiveSubDirectories(string directoryPath, List<DirectoryInfo> result)
    {
        List<DirectoryInfo> subDirectories = GetSubDirectories(directoryPath);
        result.AddRange(subDirectories);

        foreach (DirectoryInfo dir in subDirectories)
        {
            GetRecursiveSubDirectories(dir.FullName, result);
        }
    }
}
