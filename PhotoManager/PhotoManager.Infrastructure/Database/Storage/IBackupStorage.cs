namespace PhotoManager.Infrastructure.Database.Storage;

public interface IBackupStorage
{
    string[] GetBackupFilesPaths(string backupDirectory);
    void WriteFolderToZipFile(string dataDirectory, string backupFilePath);
    void DeleteBackupFile(string backupFilePath);
}
