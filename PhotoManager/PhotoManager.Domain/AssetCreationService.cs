using Microsoft.Extensions.Logging;

namespace PhotoManager.Domain;

public class AssetCreationService(
    IAssetRepository assetRepository,
    IFileOperationsService fileOperationsService,
    IImageProcessingService imageProcessingService,
    IImageMetadataService imageMetadataService,
    IAssetHashCalculatorService assetHashCalculatorService,
    IThumbnailGenerator thumbnailGenerator,
    IUserConfigurationService userConfigurationService,
    ILogger<AssetCreationService> logger)
    : IAssetCreationService
{
    public Asset? CreateAsset(string directoryName, string fileName, bool isVideo = false,
        bool skipCatalogCheck = false)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(directoryName);
            ArgumentNullException.ThrowIfNull(fileName);

            if (isVideo && userConfigurationService.AssetSettings.AnalyseVideos)
            {
                // Create an asset from the video file
                VideoHelper.GetFirstFramePath(directoryName, fileName,
                    userConfigurationService.PathSettings.FirstFrameVideosPath, logger);
                // The video file is not in the same path as the asset created
                // The asset is null because the target is not the video but the asset created previously
                return null;
            }

            string imagePath = Path.Combine(directoryName, fileName);

            if (!File.Exists(imagePath))
            {
                FileNotFoundException exception = new($"The file {imagePath} does not exist.");
                logger.LogError(exception, "{ExMessage}", exception.Message);
                return null;
            }

            if (!skipCatalogCheck && assetRepository.IsAssetCatalogued(directoryName, fileName))
            {
                return null;
            }

            AssetWithThumbnail? assetWithThumbnail = CreateAssetWithThumbnail(directoryName, fileName,
                fileOperationsService.GetFileBytes(imagePath), isVideo, skipCatalogCheck: true);

            if (assetWithThumbnail == null)
            {
                return null;
            }

            bool result = assetRepository.AddAsset(assetWithThumbnail.Asset, assetWithThumbnail.ThumbnailData);

            if (!result)
            {
                logger.LogError("The asset {AssetPath} could not be added.", assetWithThumbnail.Asset.FullPath);
            }

            return assetWithThumbnail.Asset;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{ExMessage}", ex.Message);
            return null;
        }
    }

    public AssetWithThumbnail? CreateAssetWithThumbnail(string directoryName, string fileName, byte[] imageBytes,
        bool isVideo = false, bool skipCatalogCheck = false)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(directoryName);
            ArgumentNullException.ThrowIfNull(fileName);
            ArgumentNullException.ThrowIfNull(imageBytes);

            if (isVideo && userConfigurationService.AssetSettings.AnalyseVideos)
            {
                VideoHelper.GetFirstFramePath(directoryName, fileName,
                    userConfigurationService.PathSettings.FirstFrameVideosPath, logger);
                return null;
            }

            string imagePath = Path.Combine(directoryName, fileName);

            if (!File.Exists(imagePath))
            {
                FileNotFoundException exception = new($"The file {imagePath} does not exist.");
                logger.LogError(exception, "{ExMessage}", exception.Message);
                return null;
            }

            if (!skipCatalogCheck && assetRepository.IsAssetCatalogued(directoryName, fileName))
            {
                return null;
            }

            ReadOnlySpan<char> extension = Path.GetExtension(fileName.AsSpan());

            if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
            {
                return CreateAssetFromPng(imagePath, directoryName, imageBytes);
            }

            if (extension.Equals(".gif", StringComparison.OrdinalIgnoreCase))
            {
                return CreateAssetFromGif(imagePath, directoryName, imageBytes);
            }

            if (extension.Equals(".heic", StringComparison.OrdinalIgnoreCase))
            {
                return CreateAssetFromHeic(imagePath, directoryName, imageBytes);
            }

            return CreateAssetFromOtherFormat(imagePath, directoryName, imageBytes);
        }
        catch (NotSupportedException ex)
        {
            if (IsValidImageForExtension(fileName, imageBytes))
            {
                logger.LogError(ex, "{ExMessage}", ex.Message);
            }

            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{ExMessage}", ex.Message);
            return null;
        }
    }

    private AssetWithThumbnail CreateAssetFromPng(string imagePath, string directoryName, byte[] imageBytes)
    {
        // GetExifOrientation is not handled by Png
        ushort exifOrientation = userConfigurationService.AssetSettings.DefaultExifOrientation;
        (ImageRotation rotation, bool isAssetCorrupted, bool isAssetRotated) =
            GetRotationAndCorruptionInfo(exifOrientation);
        (int originalDecodeWidth, int originalDecodeHeight) =
            imageProcessingService.GetImageDimensions(imageBytes, rotation);
        (int thumbnailDecodeWidth, int thumbnailDecodeHeight) =
            GetThumbnailDimensions(originalDecodeWidth, originalDecodeHeight);
        byte[] thumbnailBuffer = thumbnailGenerator.GenerateThumbnail(imageBytes, rotation, thumbnailDecodeWidth,
            thumbnailDecodeHeight, ImageEncodingFormat.Png);

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

    private AssetWithThumbnail CreateAssetFromGif(string imagePath, string directoryName, byte[] imageBytes)
    {
        // GetExifOrientation is not handled by GIF
        ushort exifOrientation = userConfigurationService.AssetSettings.DefaultExifOrientation;
        (ImageRotation rotation, bool isAssetCorrupted, bool isAssetRotated) =
            GetRotationAndCorruptionInfo(exifOrientation);
        (int originalDecodeWidth, int originalDecodeHeight) =
            imageProcessingService.GetImageDimensions(imageBytes, rotation);
        (int thumbnailDecodeWidth, int thumbnailDecodeHeight) =
            GetThumbnailDimensions(originalDecodeWidth, originalDecodeHeight);
        byte[] thumbnailBuffer = thumbnailGenerator.GenerateThumbnail(imageBytes, rotation, thumbnailDecodeWidth,
            thumbnailDecodeHeight, ImageEncodingFormat.Gif);

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

    private AssetWithThumbnail CreateAssetFromHeic(string imagePath, string directoryName, byte[] imageBytes)
    {
        ushort exifOrientation = imageMetadataService.GetHeicExifOrientation(imageBytes,
            userConfigurationService.AssetSettings.CorruptedImageOrientation);
        (ImageRotation rotation, bool isAssetCorrupted, bool isAssetRotated) =
            GetRotationAndCorruptionInfo(exifOrientation);
        (int originalDecodeWidth, int originalDecodeHeight) =
            imageProcessingService.GetImageDimensions(imageBytes, rotation);
        (int thumbnailDecodeWidth, int thumbnailDecodeHeight) =
            GetThumbnailDimensions(originalDecodeWidth, originalDecodeHeight);
        byte[] thumbnailBuffer = thumbnailGenerator.GenerateThumbnail(imageBytes, rotation, thumbnailDecodeWidth,
            thumbnailDecodeHeight, ImageEncodingFormat.Jpeg);

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

    private AssetWithThumbnail CreateAssetFromOtherFormat(string imagePath, string directoryName, byte[] imageBytes)
    {
        ushort exifOrientation = imageMetadataService.GetExifOrientation(
            imageBytes,
            userConfigurationService.AssetSettings.DefaultExifOrientation,
            userConfigurationService.AssetSettings.CorruptedImageOrientation);
        (ImageRotation rotation, bool isAssetCorrupted, bool isAssetRotated) =
            GetRotationAndCorruptionInfo(exifOrientation);
        (int originalDecodeWidth, int originalDecodeHeight) =
            imageProcessingService.GetImageDimensions(imageBytes, rotation);
        (int thumbnailDecodeWidth, int thumbnailDecodeHeight) =
            GetThumbnailDimensions(originalDecodeWidth, originalDecodeHeight);
        byte[] thumbnailBuffer = thumbnailGenerator.GenerateThumbnail(imageBytes, rotation, thumbnailDecodeWidth,
            thumbnailDecodeHeight, ImageEncodingFormat.Jpeg);

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

    private (ImageRotation rotation, bool assetCorrupted, bool assetRotated) GetRotationAndCorruptionInfo(
        ushort exifOrientation)
    {
        ImageRotation rotation = imageMetadataService.GetImageRotation(exifOrientation);
        bool isAssetCorrupted = exifOrientation == userConfigurationService.AssetSettings.CorruptedImageOrientation;
        bool isAssetRotated = rotation != ImageRotation.Rotate0;

        return (rotation, isAssetCorrupted, isAssetRotated);
    }

    private (int thumbnailDecodeWidth, int thumbnailDecodeHeight) GetThumbnailDimensions(int originalDecodeWidth,
        int originalDecodeHeight)
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

    private AssetWithThumbnail CreateAssetWithProperties(string imagePath, string directoryName, byte[] imageBytes,
        ImageRotation rotation, bool isAssetCorrupted, bool isAssetRotated, int originalDecodeWidth,
        int originalDecodeHeight, int thumbnailDecodeWidth, int thumbnailDecodeHeight, byte[] thumbnailBuffer)
    {
        // directoryName comes from folder in assetRepository or CatalogExistingFolder that registers the folder if not in assetRepository
        Folder folder = assetRepository.GetFolderByPath(directoryName)!;

        Asset asset = new()
        {
            FileName = Path.GetFileName(imagePath),
            FolderId = folder.Id,
            Folder = folder,
            Pixel = new()
            {
                Asset = new() { Width = originalDecodeWidth, Height = originalDecodeHeight },
                Thumbnail = new() { Width = thumbnailDecodeWidth, Height = thumbnailDecodeHeight }
            },
            ImageRotation = rotation,
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = assetHashCalculatorService.CalculateHash(imageBytes, imagePath),
            Metadata = new()
            {
                Corrupted = new()
                {
                    IsTrue = isAssetCorrupted,
                    Message = isAssetCorrupted ? userConfigurationService.AssetSettings.CorruptedMessage : null
                },
                Rotated = new()
                {
                    IsTrue = isAssetRotated,
                    Message = isAssetRotated ? userConfigurationService.AssetSettings.RotatedMessage : null
                }
            }
        };

        imageMetadataService.UpdateAssetFileProperties(asset);

        return new(asset, thumbnailBuffer);
    }

    private bool IsValidImageForExtension(string fileName, byte[] imageBytes)
    {
        ReadOnlySpan<char> extension = Path.GetExtension(fileName.AsSpan());

        return extension.Equals(".heic", StringComparison.OrdinalIgnoreCase)
            ? imageProcessingService.IsValidHeic(imageBytes)
            : imageProcessingService.IsValidImage(imageBytes);
    }
}
