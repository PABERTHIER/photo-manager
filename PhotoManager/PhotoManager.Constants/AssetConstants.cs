namespace PhotoManager.Constants;

public static class AssetConstants
{
    public const bool UsingMD5Hash = false; // Enable it to Hash in MD5
    public const bool UsingPHash = false; // Enable it to detect duplicates between rotated assets (improve detection) PHash = Perceptual Hash
    public const bool UsingDHash = false; // Enable it to Hash in DHash (Difference Hash)
    public const bool DetectThumbnails = false;
    public const ushort OrientationCorruptedImage = 10000;
    public const double MaxWidth = 200;
    public const double MaxHeight = 150;
    public const string AssetCorruptedMessage = "The asset is corrupted";
    public const string AssetRotatedMessage = "The asset has been rotated";
}