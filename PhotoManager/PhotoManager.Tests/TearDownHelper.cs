using Constants = PhotoManager.Tests.Integration.Constants;

namespace PhotoManager.Tests;

public static class TearDownHelper
{
    public static void DeleteTempDbDirectories(string databaseDirectory)
    {

        if (Directory.Exists(databaseDirectory))
        {
            Directory.Delete(databaseDirectory, true);
        }

        string backupsDirectory = databaseDirectory + Constants.DATABASE_BACKUP_END_PATH;

        if (Directory.Exists(backupsDirectory))
        {
            Directory.Delete(backupsDirectory, true);
        }
    }
}
