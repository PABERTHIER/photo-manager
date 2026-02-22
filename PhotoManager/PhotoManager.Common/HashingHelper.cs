using ImageMagick;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace PhotoManager.Common;

public static class HashingHelper
{
    public static string CalculateHash(byte[] imageBytes)
    {
        Span<byte> hash = stackalloc byte[SHA512.HashSizeInBytes];
        SHA512.HashData(imageBytes, hash);

        return string.Create(128, hash, static (chars, hashBytes) =>
        {
            for (int i = 0; i < hashBytes.Length; i++)
            {
                byte b = hashBytes[i];
                chars[i * 2] = GetHexChar(b >> 4);
                chars[(i * 2) + 1] = GetHexChar(b & 0xF);
            }
        });

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static char GetHexChar(int value) => (char)(value < 10 ? '0' + value : 'a' + value - 10);
    }

    // Performances are decreased by 6 times with CalculatePHash
    public static string? CalculatePHash(string filePath)
    {
        try
        {
            MagickImage image = new(filePath);

            // Resize the image
            MagickGeometry geometry = new(32, 32);
            image.Resize(geometry);

            // Convert the image to grayscale
            image.Grayscale(PixelIntensityMethod.Average);

            // Calculate the PHash of the image
            string? phash = image.PerceptualHash()?.ToString();

            return phash;
        }
        catch
        {
            // TODO: Log the error once the log system has been reworked
            return null;
        }
    }

    // For GIF or some heic file it returns "00000000000000"
    public static string CalculateDHash(string? filePath)
    {
        bool isHeicFile = filePath?.EndsWith(".heic", StringComparison.OrdinalIgnoreCase) ?? false;

        Bitmap? image = isHeicFile ? BitmapHelper.LoadBitmapFromPath(filePath!) : new(filePath!);

        ulong hash = 0UL;
        ulong mask = 1UL;

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 7; x++)
            {
                float? leftPixel = image?.GetPixel(x, y).GetBrightness();
                float? rightPixel = image?.GetPixel(x + 1, y).GetBrightness();
                if (leftPixel < rightPixel)
                {
                    hash |= mask;
                }
                mask <<= 1;
            }
        }

        return hash.ToString("x14"); // Always 14 hex chars (lowercase)
    }

    public static string CalculateMD5Hash(byte[] imageBytes)
    {
        byte[] hashBytes = MD5.HashData(imageBytes);
        string md5Hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        return md5Hash;
    }

    // The best use is for PHash method, the most accurate
    public static int CalculateHammingDistance(string hash1, string hash2)
    {
        if (hash1 == null || hash2 == null || hash1.Length != hash2.Length)
        {
            // TODO: Check for ArgumentException. - .net 8) to not throw
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
