using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace PhotoManager.Infrastructure;

public class FileOperationsService(
    IUserConfigurationService userConfigurationService,
    ILogger<FileOperationsService> logger)
    : IFileOperationsService
{
    public DirectoryInfo[] GetSubDirectories(string directoryPath)
    {
        DirectoryInfo[] directories = new DirectoryInfo(directoryPath).GetDirectories();
        Array.Sort(directories,
            static (left, right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));
        return directories;
    }

    public DirectoryInfo[] GetRecursiveSubDirectories(string directoryPath)
    {
        DirectoryInfo[] directories = new DirectoryInfo(directoryPath).GetDirectories("*", SearchOption.AllDirectories);
        Array.Sort(directories,
            static (left, right) => string.Compare(left.FullName, right.FullName, StringComparison.OrdinalIgnoreCase));
        return directories;
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
            try
            {
                File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete file '{FilePath}'. {Message}", fullPath, ex.Message);
            }
        }
    }

    public string[] GetFileNames(string directory)
    {
        string[] fileNames = Directory.GetFiles(directory);

        for (int i = 0; i < fileNames.Length; i++)
        {
            fileNames[i] = Path.GetFileName(fileNames[i]);
        }

        return fileNames;
    }

    public FileInfo[] GetFileInfos(string directory)
    {
        return new DirectoryInfo(directory).GetFiles();
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
