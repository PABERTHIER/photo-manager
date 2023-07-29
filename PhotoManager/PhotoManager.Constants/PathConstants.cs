namespace PhotoManager.Constants;

public static class PathConstants
{
    //public const string PathLocation = "D:\\Out\\Photos";
    //public const string PathBackup = "D:\\Workspace\\PhotoManager\\Backup";


    // MEMUP location
    //public const string PathLocation = "F:\\Xperia Z1";
    //public const string PathBackup = "F:\\Xperia Z1\\Backup";


    // MEMUP V1 location
    //public const string PathLocation = "F:\\Xperia Z1\\V1";
    //public const string PathBackup = "F:\\Xperia Z1\\V1\\Backup";

    // Test
    public const string PathLocation = "D:\\Workspace\\PhotoManager\\Test"; // The path where PhotoManager will scan your assets.
    public const string PathLocationToExemptTheFolder = "D:\\Workspace\\PhotoManager\\Test\\test1"; // The path where PhotoManager will protect your assets and if there are duplicates in others path, you will be able to delete all of them except the asset in this exempted path
    public const string FfmpegPath = "E:\\ffmpeg\\bin\\ffmpeg.exe"; // The path where your ffmpeg.exe is located (needed to be installed first, used if you activate the switch `AnalyseVideos`)
    // The backup must be upper than the location to prevent bugs like "Process is already used"
    public const string PathBackup = "D:\\Workspace\\PhotoManager\\Backup"; // The path to store your backup
    public const string PathBackupTests = "D:\\Workspace\\PhotoManager\\BackupTests"; // The path to store your backup Tests
}
