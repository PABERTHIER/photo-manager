namespace PhotoManager.Constants;

public static class PathConstants
{
    // The path where PhotoManager will scan your assets
    public const string PathLocation = "D:\\Workspace\\PhotoManager\\Test";
    // The path where PhotoManager will protect your assets and if there are duplicates in others path, you will be able to delete all of them except the asset in this exempted path
    public const string PathLocationToExemptTheFolder = "D:\\Workspace\\PhotoManager\\Test\\test1";
    // The path to save the first frame for each video file
    public const string PathFirstFrameVideos = PathLocation + "\\OutputVideoFirstFrame";
    // The path where your ffmpeg.exe is located (needed to be installed first, used if you activate the switch `AnalyseVideos`)
    public const string FfmpegPath = "E:\\ffmpeg\\bin\\ffmpeg.exe";

    // The backup must be upper than the location to prevent bugs like "Process is already used"
    public const string PathBackup = "D:\\Workspace\\PhotoManager\\Backup"; // The path to store your backup
}
