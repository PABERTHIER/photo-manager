namespace PhotoManager.Constants;

public static class PathConstants
{
    //public const string PathLocation = "D:\\Out\\Photos";
    //public const string PathBackUp = "D:\\Workspace\\PhotoManager\\BackUp";


    // MEMUP location
    //public const string PathLocation = "F:\\Xperia Z1";
    //public const string PathBackUp = "F:\\Xperia Z1\\BackUp";


    // MEMUP V1 location
    //public const string PathLocation = "F:\\Xperia Z1\\V1";
    //public const string PathBackUp = "F:\\Xperia Z1\\V1\\BackUp";

    // Test
    public const string PathLocation = "D:\\Workspace\\PhotoManager\\Test";
    public const string PathLocationToExemptTheFolder = "D:\\Workspace\\PhotoManager\\Test\\test1";
    // The backup must be upper than the location to prevent bugs like "Process is already used"
    public const string PathBackUp = "D:\\Workspace\\PhotoManager\\BackUp";
    public const string PathBackUpTests = "D:\\Workspace\\PhotoManager\\BackUpTests";
}
