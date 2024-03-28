namespace PhotoManager.Infrastructure;

public class AssetHashCalculatorService(IUserConfigurationService userConfigurationService) : IAssetHashCalculatorService
{
    public string CalculateHash(byte[] imageBytes, string filePath)
    {
        if (userConfigurationService.HashSettings.UsingPHash)
        {
            return HashingHelper.CalculatePHash(filePath) ?? HashingHelper.CalculateHash(imageBytes);
        }

        if (userConfigurationService.HashSettings.UsingDHash)
        {
            return HashingHelper.CalculateDHash(filePath);
        }

        if (userConfigurationService.HashSettings.UsingMD5Hash)
        {
            return HashingHelper.CalculateMD5Hash(imageBytes);
        }

        return HashingHelper.CalculateHash(imageBytes);
    }
}
