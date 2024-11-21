using log4net;
using System.Reflection;

namespace PhotoManager.Domain;

public class AssetCreationService(
    IAssetRepository assetRepository,
    IStorageService storageService,
    IAssetHashCalculatorService assetHashCalculatorService,
    IUserConfigurationService userConfigurationService)
    : IAssetCreationService
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public Asset? CreateAsset(string directoryName, string fileName, bool isVideo = false)
    {
        if (isVideo && userConfigurationService.AssetSettings.AnalyseVideos)
        {
            // Create an asset from the video file
            VideoHelper.GetFirstFramePath(directoryName, fileName, userConfigurationService.PathSettings.FirstFrameVideosPath);
            // The video file is not in the same path as the asset created
            // The asset is null because the target is not the video but the asset created previously
            return null;
        }

        string imagePath = Path.Combine(directoryName, fileName);

        if (!File.Exists(imagePath))
        {
            Log.Error(new FileNotFoundException($"The file {imagePath} does not exist."));
            return null;
        }

        if (assetRepository.IsAssetCatalogued(directoryName, fileName))
        {
            return null;
        }

        byte[] imageBytes = storageService.GetFileBytes(imagePath);

        return Path.GetExtension(fileName).ToLower() switch
        {
            ".png" => CreateAssetFromPng(imagePath, directoryName, imageBytes),
            ".gif" => CreateAssetFromGif(imagePath, directoryName, imageBytes),
            ".heic" => CreateAssetFromHeic(imagePath, directoryName, imageBytes),
            _ => CreateAssetFromOtherFormat(imagePath, directoryName, imageBytes)
        };
    }

    private Asset? CreateAssetFromPng(string imagePath, string directoryName, byte[] imageBytes)
    {
        if (!storageService.IsValidGDIPlusImage(imageBytes))
        {
            return null;
        }

        ushort exifOrientation = userConfigurationService.AssetSettings.DefaultExifOrientation; // GetExifOrientation is not handled by Png
        (Rotation rotation, bool isAssetCorrupted, bool isAssetRotated) = GetRotationAndCorruptionInfo(exifOrientation);
        BitmapImage originalImage = storageService.LoadBitmapOriginalImage(imageBytes, rotation);
        (int originalDecodeWidth, int originalDecodeHeight) = GetOriginalDecodeLengths(originalImage);
        (int thumbnailDecodeWidth, int thumbnailDecodeHeight) = GetThumbnailDimensions(originalDecodeWidth, originalDecodeHeight);
        BitmapImage thumbnailImage = storageService.LoadBitmapThumbnailImage(imageBytes, rotation, thumbnailDecodeWidth, thumbnailDecodeHeight);
        byte[] thumbnailBuffer = storageService.GetPngBitmapImage(thumbnailImage);

        return CreateAssetWithProperties(
            imagePath,
            directoryName,
            imageBytes,
            rotation,
            isAssetCorrupted,
            isAssetRotated,
            originalDecodeWidth,
            originalDecodeHeight,
            thumbnailDecodeWidth,
            thumbnailDecodeHeight,
            thumbnailBuffer);
    }

    private Asset? CreateAssetFromGif(string imagePath, string directoryName, byte[] imageBytes)
    {
        if (!storageService.IsValidGDIPlusImage(imageBytes))
        {
            return null;
        }

        ushort exifOrientation = userConfigurationService.AssetSettings.DefaultExifOrientation; // GetExifOrientation is not handled by Gif
        (Rotation rotation, bool isAssetCorrupted, bool isAssetRotated) = GetRotationAndCorruptionInfo(exifOrientation);
        BitmapImage originalImage = storageService.LoadBitmapOriginalImage(imageBytes, rotation);
        (int originalDecodeWidth, int originalDecodeHeight) = GetOriginalDecodeLengths(originalImage);
        (int thumbnailDecodeWidth, int thumbnailDecodeHeight) = GetThumbnailDimensions(originalDecodeWidth, originalDecodeHeight);
        BitmapImage thumbnailImage = storageService.LoadBitmapThumbnailImage(imageBytes, rotation, thumbnailDecodeWidth, thumbnailDecodeHeight);
        byte[] thumbnailBuffer = storageService.GetGifBitmapImage(thumbnailImage);

        return CreateAssetWithProperties(
            imagePath,
            directoryName,
            imageBytes,
            rotation,
            isAssetCorrupted, 
            isAssetRotated,
            originalDecodeWidth,
            originalDecodeHeight,
            thumbnailDecodeWidth,
            thumbnailDecodeHeight,
            thumbnailBuffer);
    }

    private Asset? CreateAssetFromHeic(string imagePath, string directoryName, byte[] imageBytes)
    {
        if (!storageService.IsValidHeic(imageBytes))
        {
            return null;
        }

        ushort exifOrientation = storageService.GetHeicExifOrientation(imageBytes, userConfigurationService.AssetSettings.CorruptedImageOrientation);
        (Rotation rotation, bool isAssetCorrupted, bool isAssetRotated) = GetRotationAndCorruptionInfo(exifOrientation);
        BitmapImage originalImage = storageService.LoadBitmapHeicOriginalImage(imageBytes, rotation);
        (int originalDecodeWidth, int originalDecodeHeight) = GetOriginalDecodeLengths(originalImage);
        (int thumbnailDecodeWidth, int thumbnailDecodeHeight) = GetThumbnailDimensions(originalDecodeWidth, originalDecodeHeight);
        BitmapImage thumbnailImage = storageService.LoadBitmapHeicThumbnailImage(imageBytes, rotation, thumbnailDecodeWidth, thumbnailDecodeHeight);
        byte[] thumbnailBuffer = storageService.GetJpegBitmapImage(thumbnailImage);

        return CreateAssetWithProperties(
            imagePath,
            directoryName,
            imageBytes,
            rotation,
            isAssetCorrupted,
            isAssetRotated,
            originalDecodeWidth,
            originalDecodeHeight,
            thumbnailDecodeWidth,
            thumbnailDecodeHeight,
            thumbnailBuffer);
    }

    private Asset? CreateAssetFromOtherFormat(string imagePath, string directoryName, byte[] imageBytes)
    {
        if (!storageService.IsValidGDIPlusImage(imageBytes))
        {
            return null;
        }

        ushort exifOrientation = storageService.GetExifOrientation(
            imageBytes,
            userConfigurationService.AssetSettings.DefaultExifOrientation,
            userConfigurationService.AssetSettings.CorruptedImageOrientation);
        (Rotation rotation, bool isAssetCorrupted, bool isAssetRotated) = GetRotationAndCorruptionInfo(exifOrientation);
        BitmapImage originalImage = storageService.LoadBitmapOriginalImage(imageBytes, rotation);
        (int originalDecodeWidth, int originalDecodeHeight) = GetOriginalDecodeLengths(originalImage);
        (int thumbnailDecodeWidth, int thumbnailDecodeHeight) = GetThumbnailDimensions(originalDecodeWidth, originalDecodeHeight);
        BitmapImage thumbnailImage = storageService.LoadBitmapThumbnailImage(imageBytes, rotation, thumbnailDecodeWidth, thumbnailDecodeHeight);
        byte[] thumbnailBuffer = storageService.GetJpegBitmapImage(thumbnailImage);

        return CreateAssetWithProperties(
            imagePath,
            directoryName,
            imageBytes,
            rotation,
            isAssetCorrupted,
            isAssetRotated,
            originalDecodeWidth,
            originalDecodeHeight,
            thumbnailDecodeWidth,
            thumbnailDecodeHeight,
            thumbnailBuffer);
    }

    private (Rotation rotation, bool assetCorrupted, bool assetRotated) GetRotationAndCorruptionInfo(ushort exifOrientation)
    {
        Rotation rotation = storageService.GetImageRotation(exifOrientation);
        bool isAssetCorrupted = exifOrientation == userConfigurationService.AssetSettings.CorruptedImageOrientation;
        bool isAssetRotated = rotation != Rotation.Rotate0;

        return (rotation, isAssetCorrupted, isAssetRotated);
    }

    private static (int originalDecodeWidth, int originalDecodeHeight) GetOriginalDecodeLengths(BitmapImage originalImage)
    {
        int originalDecodeWidth = originalImage.PixelWidth;
        int originalDecodeHeight = originalImage.PixelHeight;

        return (originalDecodeWidth, originalDecodeHeight);
    }

    private (int thumbnailDecodeWidth, int thumbnailDecodeHeight) GetThumbnailDimensions(int originalDecodeWidth, int originalDecodeHeight)
    {
        int thumbnailDecodeWidth;
        int thumbnailDecodeHeight;
        float percentage;

        // If the original image is landscape
        if (originalDecodeWidth > originalDecodeHeight)
        {
            int thumbnailMaxWidth = userConfigurationService.AssetSettings.ThumbnailMaxWidth;
            thumbnailDecodeWidth = thumbnailMaxWidth;
            percentage = thumbnailMaxWidth * 100f / originalDecodeWidth;
            thumbnailDecodeHeight = Convert.ToInt32(percentage * originalDecodeHeight / 100);
        }
        else // If the original image is portrait
        {
            int thumbnailMaxHeight = userConfigurationService.AssetSettings.ThumbnailMaxHeight;
            thumbnailDecodeHeight = thumbnailMaxHeight;
            percentage = thumbnailMaxHeight * 100f / originalDecodeHeight;
            thumbnailDecodeWidth = Convert.ToInt32(percentage * originalDecodeWidth / 100);
        }

        return (thumbnailDecodeWidth, thumbnailDecodeHeight);
    }

    private Asset CreateAssetWithProperties(string imagePath, string directoryName, byte[] imageBytes, Rotation rotation, bool isAssetCorrupted, bool isAssetRotated, int originalDecodeWidth, int originalDecodeHeight, int thumbnailDecodeWidth, int thumbnailDecodeHeight, byte[] thumbnailBuffer)
    {
        // directoryName comes from folder in assetRepository or CatalogExistingFolder that registers the folder if not in assetRepository
        Folder folder = assetRepository.GetFolderByPath(directoryName)!;

        Asset asset = new()
        {
            FileName = Path.GetFileName(imagePath),
            FolderId = folder.FolderId,
            Folder = folder,
            FileSize = new FileInfo(imagePath).Length,
            PixelWidth = originalDecodeWidth,
            PixelHeight = originalDecodeHeight,
            ThumbnailPixelWidth = thumbnailDecodeWidth,
            ThumbnailPixelHeight = thumbnailDecodeHeight,
            ImageRotation = rotation,
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = assetHashCalculatorService.CalculateHash(imageBytes, imagePath),
            IsAssetCorrupted = isAssetCorrupted,
            AssetCorruptedMessage = isAssetCorrupted ? userConfigurationService.AssetSettings.AssetCorruptedMessage : null,
            IsAssetRotated = isAssetRotated,
            AssetRotatedMessage = isAssetRotated ? userConfigurationService.AssetSettings.AssetRotatedMessage : null,
        };

        assetRepository.AddAsset(asset, thumbnailBuffer);

        return asset;
    }
}
