namespace PhotoManager.Infrastructure;

public class AssetHashCalculatorService(IUserConfigurationService userConfigurationService) : IAssetHashCalculatorService
{
    private readonly IUserConfigurationService _userConfigurationService = userConfigurationService;

    public string CalculateHash(byte[] imageBytes, string filePath)
    {
        if (_userConfigurationService.HashSettings.UsingPHash)
        {
            return HashingHelper.CalculatePHash(filePath) ?? HashingHelper.CalculateHash(imageBytes);
        }

        if (_userConfigurationService.HashSettings.UsingDHash)
        {
            return HashingHelper.CalculateDHash(filePath) ?? HashingHelper.CalculateHash(imageBytes);
        }

        if (_userConfigurationService.HashSettings.UsingMD5Hash)
        {
            return HashingHelper.CalculateMD5Hash(imageBytes) ?? HashingHelper.CalculateHash(imageBytes);
        }

        return HashingHelper.CalculateHash(imageBytes);
    }
}
