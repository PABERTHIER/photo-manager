namespace PhotoManager.Infrastructure;

public class AssetHashCalculatorService : IAssetHashCalculatorService
{
    public string CalculateHash(byte[] imageBytes, string filePath)
    {
#pragma warning disable CS0162 // Unreachable code detected
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
#pragma warning restore CS0162 // Unreachable code detected
    }
}
