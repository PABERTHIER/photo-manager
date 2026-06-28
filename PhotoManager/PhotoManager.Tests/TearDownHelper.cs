namespace PhotoManager.Tests;

public static class TearDownHelper
{
    public static void DeleteTempDbDirectories(string databaseDirectory)
    {
        if (Directory.Exists(databaseDirectory))
        {
            Directory.Delete(databaseDirectory, true);
        }
    }
}
