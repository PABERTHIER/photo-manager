﻿namespace PhotoManager.Constants;

public static class AssetConstants
{
    public const bool UsingPHash = false; // Enable it to detect duplicates between rotated assets (improve detection)
    public const ushort OrientationCorruptedImage = 10000;
    public const double MaxWidth = 200;
    public const double MaxHeight = 150;
    public const string AssetCorruptedMessage = "The asset is corrupted";
    public const string AssetRotatedMessage = "The asset has been rotated";
}