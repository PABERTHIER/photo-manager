using System.IO;
using System.IO.Compression;

namespace PhotoManager.Infrastructure.Database.Storage;

public class BackupStorage : IBackupStorage
{
    public string[] GetBackupFilesPaths(string backupDirectory)
    {
        return Directory.GetFiles(backupDirectory);
    }

    public void WriteFolderToZipFile(string sourceDirectoryName, string destinationArchiveFileName)
    {
        ZipFile.CreateFromDirectory(sourceDirectoryName, destinationArchiveFileName, CompressionLevel.NoCompression, false);
    }

    public void DeleteBackupFile(string backupFilePath)
    {
        File.Delete(backupFilePath);
    }
}
