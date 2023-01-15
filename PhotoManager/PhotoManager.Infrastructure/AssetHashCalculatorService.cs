using PhotoManager.Domain.Interfaces;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PhotoManager.Infrastructure;

public class AssetHashCalculatorService : IAssetHashCalculatorService
{
    public string CalculateHash(byte[] imageBytes)
    {
        StringBuilder hashBuilder = new();
        byte[] hash = SHA512.Create().ComputeHash(imageBytes);

        foreach (byte hashByte in hash)
        {
            hashBuilder.Append(string.Format("{0:x2}", hashByte));
        }

        return hashBuilder.ToString();
    }

    public string CalculateVideoHash(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);
        using SHA1Managed sha = new();
        byte[] hash = sha.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }
}
