using System.Collections.Concurrent;

namespace PhotoManager.Infrastructure;

public class FileOperationsService(IUserConfigurationService userConfigurationService) : IFileOperationsService
{
    public List<DirectoryInfo> GetSubDirectories(string directoryPath)
    {
        return [.. new DirectoryInfo(directoryPath).EnumerateDirectories()];
    }

    public List<DirectoryInfo> GetRecursiveSubDirectories(string directoryPath)
    {
        return [.. new DirectoryInfo(directoryPath).EnumerateDirectories("*", SearchOption.AllDirectories)];
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
        return [.. files.Select(f => Path.GetFileName(f))];
    }

    public byte[] GetFileBytes(string filePath)
    {
        return File.ReadAllBytes(filePath);
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

    private static int CountFilesInDirectory(string directoryPath, SearchOption searchOption)
    {
        return Directory.EnumerateFiles(directoryPath, "*", searchOption).Count();
    }
}
