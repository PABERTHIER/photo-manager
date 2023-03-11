using PhotoManager.Common;
using PhotoManager.Constants;
using PhotoManager.Domain.Interfaces;
using System.IO;
using System.Security.Cryptography;

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

    // TODO: Take the first frame to call then the CalculateHash method ? + Use HashingHelper
    public string CalculateVideoHash(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);
        using SHA1Managed sha = new();
        byte[] hash = sha.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }
}
