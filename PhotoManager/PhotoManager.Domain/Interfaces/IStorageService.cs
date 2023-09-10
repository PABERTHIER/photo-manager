namespace PhotoManager.Domain.Interfaces;

public interface IStorageService
{
    List<DirectoryInfo> GetSubDirectories(string directoryPath);
    List<DirectoryInfo> GetRecursiveSubDirectories(string directoryPath);
    string ResolveDataDirectory(double storageVersion);
    void CreateDirectory(string directory);
    void DeleteFile(string directory, string fileName);
    string[] GetFileNames(string directory);
    byte[] GetFileBytes(string filePath);
    BitmapImage LoadBitmapThumbnailImage(byte[] buffer, Rotation rotation, int width, int height);
    BitmapImage LoadBitmapThumbnailImage(byte[] buffer, int width, int height);
    BitmapImage LoadBitmapOriginalImage(byte[] buffer, Rotation rotation);
    BitmapImage LoadBitmapImageFromPath(string imagePath, Rotation rotation);
    BitmapImage LoadBitmapHeicOriginalImage(byte[] buffer, Rotation rotation);
    BitmapImage LoadBitmapHeicThumbnailImage(byte[] buffer, Rotation rotation, int width, int height);
    BitmapImage LoadBitmapHeicImageFromPath(string imagePath, Rotation rotation);
    ushort GetExifOrientation(byte[] buffer);
    ushort GetHeicExifOrientation(byte[] buffer);
    Rotation GetImageRotation(ushort exifOrientation);
    //bool HasSameContent(Asset assetA, Asset assetB);
    byte[] GetJpegBitmapImage(BitmapImage thumbnailImage);
    byte[] GetPngBitmapImage(BitmapImage thumbnailImage);
    byte[] GetGifBitmapImage(BitmapImage thumbnailImage);
    bool FileExists(Asset asset, Folder folder);
    bool FileExists(string fullPath);
    bool FolderExists(string fullPath);
    void LoadFileInformation(Asset asset);
    bool IsValidGDIPlusImage(byte[] imageData);
    bool IsValidHeic(byte[] imageData);
}
