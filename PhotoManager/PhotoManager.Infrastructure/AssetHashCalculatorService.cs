using PhotoManager.Common;
using PhotoManager.Constants;
using PhotoManager.Domain.Interfaces;

namespace PhotoManager.Infrastructure;

public class AssetHashCalculatorService : IAssetHashCalculatorService
{
    public string CalculateHash(byte[] imageBytes, string filePath)
    {
        if (AssetConstants.UsingPHash)
        {
            return HashingHelper.CalculatePHash(filePath) ?? HashingHelper.CalculateHash(imageBytes);
        }

        if (AssetConstants.UsingDHash)
        {
            return HashingHelper.CalculateDHash(filePath) ?? HashingHelper.CalculateHash(imageBytes);
        }

        if (AssetConstants.UsingMD5Hash)
        {
            return HashingHelper.CalculateMD5Hash(filePath) ?? HashingHelper.CalculateHash(imageBytes);
        }

        return HashingHelper.CalculateHash(imageBytes);
    }
}
