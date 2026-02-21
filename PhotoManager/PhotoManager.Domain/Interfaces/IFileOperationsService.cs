namespace PhotoManager.Domain.Interfaces;

public interface IFileOperationsService
{
    List<DirectoryInfo> GetSubDirectories(string directoryPath);
    List<DirectoryInfo> GetRecursiveSubDirectories(string directoryPath);
    void CreateDirectory(string directory);
    void DeleteFile(string directory, string fileName);
    string[] GetFileNames(string directory);
    byte[] GetFileBytes(string filePath);
    bool FileExists(Folder folder, Asset asset);
    bool FileExists(string fullPath);
    bool FolderExists(string fullPath);
    int GetTotalFilesCount();
}
