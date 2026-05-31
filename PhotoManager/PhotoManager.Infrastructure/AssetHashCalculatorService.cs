using Microsoft.Extensions.Logging;

namespace PhotoManager.Infrastructure;

public class AssetHashCalculatorService(
    IUserConfigurationService userConfigurationService,
    ILogger<AssetHashCalculatorService> logger)
    : IAssetHashCalculatorService
{
    public string CalculateHash(byte[] imageBytes, string filePath)
    {
        if (userConfigurationService.HashSettings.UsingPHash)
        {
            return HashingHelper.CalculatePHash(filePath, logger) ?? CalculateDefaultHash(imageBytes, filePath);
        }

        if (userConfigurationService.HashSettings.UsingDHash)
        {
            return HashingHelper.CalculateDHash(filePath);
        }

        if (userConfigurationService.HashSettings.UsingMD5Hash)
        {
            return CalculateMd5Hash(imageBytes, filePath);
        }

        return CalculateDefaultHash(imageBytes, filePath);
    }

    private static string CalculateDefaultHash(byte[] imageBytes, string filePath)
    {
        if (HasImageBytes(imageBytes) || !File.Exists(filePath))
        {
            return HashingHelper.CalculateHash(imageBytes);
        }

        return HashingHelper.CalculateHash(filePath);
    }

    private static string CalculateMd5Hash(byte[] imageBytes, string filePath)
    {
        if (HasImageBytes(imageBytes) || !File.Exists(filePath))
        {
            return HashingHelper.CalculateMD5Hash(imageBytes);
        }

        return HashingHelper.CalculateMD5Hash(filePath);
    }

    private static bool HasImageBytes(byte[] imageBytes) => imageBytes is { Length: > 0 };
}
