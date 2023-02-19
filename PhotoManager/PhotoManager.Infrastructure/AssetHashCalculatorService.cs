using ImageMagick;
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

    // TODO: Take the first frame to call then the CalculatePHash method ?
    public string CalculateVideoHash(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);
        using SHA1Managed sha = new();
        byte[] hash = sha.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }

    public string? CalculatePHash(string filePath)
    {
        var image = new MagickImage(filePath);

        // Resize the image
        var size = new MagickGeometry(32, 32);
        image.Resize(size);

        // Convert the image to grayscale
        image.Grayscale(PixelIntensityMethod.Average);

        // Calculate the PHash of the image
        var phash = image.PerceptualHash()?.ToString();
        return phash;
    }
}
