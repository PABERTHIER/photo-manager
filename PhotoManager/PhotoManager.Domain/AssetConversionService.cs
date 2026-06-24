using Microsoft.Extensions.Logging;

namespace PhotoManager.Domain;

public class AssetConversionService(IFileOperationsService fileOperationsService,
    IImageProcessingService imageProcessingService, ILogger<AssetConversionService> logger)
    : IAssetConversionService
{
    private const string CONVERTED_FOLDER_NAME = "Converted";

    public async Task<AssetConversionResult> ConvertAssetsAsync(AssetConversionRequest request,
        AssetConversionProgressCallback callback, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(callback);

        if (request.FilePaths.Count == 0)
        {
            logger.LogError("Cannot convert assets because no file has been selected.");
            throw new ArgumentException("At least one file must be selected.", nameof(request));
        }

        return await Task.Run(() => ConvertAssets(request, callback, token), token);
    }

    private AssetConversionResult ConvertAssets(AssetConversionRequest request,
        AssetConversionProgressCallback callback, CancellationToken token)
    {
        List<AssetConversionItemResult> items = new(request.FilePaths.Count);

        for (int i = 0; i < request.FilePaths.Count; i++)
        {
            token.ThrowIfCancellationRequested();

            string sourcePath = request.FilePaths[i];
            AssetConversionItemResult item = ConvertAsset(sourcePath, request.TargetFormat);
            items.Add(item);

            callback(new()
            {
                CompletedCount = i + 1,
                TotalCount = request.FilePaths.Count,
                Message = item.Message
            });
        }

        return new(items);
    }

    private AssetConversionItemResult ConvertAsset(string sourcePath, AssetConversionTargetFormat targetFormat)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                logger.LogError("Cannot convert an asset because the source path is null or empty.");
                return new(sourcePath, null, false, "The source path is empty.");
            }

            if (!fileOperationsService.FileExists(sourcePath))
            {
                logger.LogError("Cannot convert '{sourcePath}' because the file does not exist.", sourcePath);
                return new(sourcePath, null, false, $"File not found: {sourcePath}");
            }

            if (!IsSupportedConversion(sourcePath, targetFormat))
            {
                string targetExtension = GetTargetExtension(targetFormat);
                string message = $"{Path.GetFileName(sourcePath)} cannot be converted to {targetExtension}.";
                logger.LogError("{Message}", message);

                return new(sourcePath, null, false, message);
            }

            string destinationPath = GetDestinationPath(sourcePath, targetFormat);
            string destinationDirectory = Path.GetDirectoryName(destinationPath)!;

            if (!fileOperationsService.FolderExists(destinationDirectory))
            {
                fileOperationsService.CreateDirectory(destinationDirectory);
            }

            byte[] convertedBytes = imageProcessingService.ConvertImage(sourcePath, ToImageEncodingFormat(targetFormat));
            fileOperationsService.WriteFileBytes(destinationPath, convertedBytes);

            return new(sourcePath, destinationPath, true, $"{Path.GetFileName(sourcePath)} converted.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cannot convert '{sourcePath}': {Message}", sourcePath, ex.Message);

            return new(sourcePath, null, false, $"Could not convert {Path.GetFileName(sourcePath)}.");
        }
    }

    private static string GetDestinationPath(string sourcePath, AssetConversionTargetFormat targetFormat)
    {
        string sourceDirectory = Path.GetDirectoryName(sourcePath)!;
        string convertedDirectory = Path.Combine(sourceDirectory, CONVERTED_FOLDER_NAME);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourcePath);

        return Path.Combine(convertedDirectory, fileNameWithoutExtension + GetTargetExtension(targetFormat));
    }

    private static bool IsSupportedConversion(string sourcePath, AssetConversionTargetFormat targetFormat)
    {
        string extension = Path.GetExtension(sourcePath);

        return targetFormat switch
        {
            AssetConversionTargetFormat.Jpeg => IsHeic(extension) || IsPng(extension),
            AssetConversionTargetFormat.Png => IsHeic(extension) || IsJpeg(extension),
            _ => false
        };
    }

    private static ImageEncodingFormat ToImageEncodingFormat(AssetConversionTargetFormat targetFormat) =>
        targetFormat == AssetConversionTargetFormat.Png
            ? ImageEncodingFormat.Png
            : ImageEncodingFormat.Jpeg;

    private static string GetTargetExtension(AssetConversionTargetFormat targetFormat) =>
        targetFormat switch
        {
            AssetConversionTargetFormat.Jpeg => ".jpg",
            AssetConversionTargetFormat.Png => ".png",
            _ => throw new ArgumentOutOfRangeException(nameof(targetFormat), targetFormat, "Unknown target format.")
        };

    private static bool IsHeic(string extension) =>
        extension.Equals(".heic", StringComparison.OrdinalIgnoreCase)
        || extension.Equals(".heif", StringComparison.OrdinalIgnoreCase);

    private static bool IsPng(string extension) =>
        extension.Equals(".png", StringComparison.OrdinalIgnoreCase);

    private static bool IsJpeg(string extension) =>
        extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
        || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase);
}
