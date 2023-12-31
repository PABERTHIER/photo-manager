using System.Globalization;

namespace PhotoManager.Infrastructure;

public class StorageService : IStorageService
{
    private readonly IUserConfigurationService _userConfigurationService;

    public StorageService(IUserConfigurationService userConfigurationService)
    {
        _userConfigurationService = userConfigurationService;
    }

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
        return Path.Combine(_userConfigurationService.PathSettings.BackupPath, "v" + storageVersion.ToString("0.0", CultureInfo.InvariantCulture));
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

    public void LoadFileInformation(Asset asset)
    {
        if (FileExists(asset.FullPath))
        {
            FileInfo info = new (asset.FullPath);
            asset.FileCreationDateTime = info.CreationTime;
            asset.FileModificationDateTime = info.LastWriteTime;
        }
    }

    public bool IsValidGDIPlusImage(byte[] imageData)
    {
        return ExifHelper.IsValidGDIPlusImage(imageData);
    }

    public bool IsValidHeic(byte[] imageData)
    {
        return ExifHelper.IsValidHeic(imageData);
    }

    private void GetRecursiveSubDirectories(string directoryPath, List<DirectoryInfo> result)
    {
        List<DirectoryInfo> subdirs = GetSubDirectories(directoryPath);
        result.AddRange(subdirs);

        foreach (DirectoryInfo dir in subdirs)
        {
            GetRecursiveSubDirectories(dir.FullName, result);
        }
    }
}
