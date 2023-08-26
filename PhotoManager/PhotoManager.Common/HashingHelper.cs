using ImageMagick;
using System.Security.Cryptography;
using System.Text;

namespace PhotoManager.Common;

public static class HashingHelper
{
    public static string CalculateHash(byte[] imageBytes)
    {
        StringBuilder hashBuilder = new();
        byte[] hash = SHA512.Create().ComputeHash(imageBytes);

        foreach (byte hashByte in hash)
        {
            hashBuilder.Append(string.Format("{0:x2}", hashByte));
        }

        return hashBuilder.ToString();
    }

    // Performances are decreased by 6 times with CalculatePHash
    public static string? CalculatePHash(string filePath)
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

    // For Gif file it returns "0"
    public static string CalculateDHash(string filePath)
    {
        Bitmap? image;
        bool isHeicFile = filePath?.EndsWith(".heic", StringComparison.OrdinalIgnoreCase) ?? false;

        image = isHeicFile ? BitmapHelper.LoadBitmapFromPath(filePath!) : new Bitmap(filePath!);

        var hash = 0UL;
        var mask = 1UL;

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 7; x++)
            {
                var leftPixel = image?.GetPixel(x, y).GetBrightness();
                var rightPixel = image?.GetPixel(x + 1, y).GetBrightness();
                if (leftPixel < rightPixel)
                {
                    hash |= mask;
                }
                mask <<= 1;
            }
        }

        return hash.ToString();
    }

    public static string CalculateMD5Hash(string filePath)
    {
        using (var stream = File.OpenRead(filePath))
        {
            var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(stream);
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            return hashString;
        }
    }

    //The best use is for PHash method, the more accurate
    public static int CalculateHammingDistance(string hash1, string hash2)
    {
        if (hash1 == null || hash2 == null || hash1.Length != hash2.Length)
        {
            throw new ArgumentException("Invalid arguments for hamming distance calculation.");
        }

        int distance = 0;

        for (int i = 0; i < hash1.Length; i++)
        {
            if (hash1[i] != hash2[i])
            {
                distance++;
            }
        }

        return distance;
    }
}
