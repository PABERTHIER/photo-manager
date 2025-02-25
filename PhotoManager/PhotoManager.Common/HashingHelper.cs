﻿using ImageMagick;
using System.Security.Cryptography;
using System.Text;

namespace PhotoManager.Common;

public static class HashingHelper
{
    public static string CalculateHash(byte[] imageBytes)
    {
        StringBuilder hashBuilder = new();
        byte[] hash = SHA512.HashData(imageBytes);

        foreach (byte hashByte in hash)
        {
            hashBuilder.Append($"{hashByte:x2}");
        }

        return hashBuilder.ToString();
    }

    // Performances are decreased by 6 times with CalculatePHash
    public static string? CalculatePHash(string filePath)
    {
        MagickImage image = new (filePath);

        // Resize the image
        MagickGeometry geometry = new (32, 32);
        image.Resize(geometry);

        // Convert the image to grayscale
        image.Grayscale(PixelIntensityMethod.Average);

        // Calculate the PHash of the image
        string? phash = image.PerceptualHash()?.ToString();

        return phash;
    }

    // For Gif or some heic file it returns "0"
    public static string CalculateDHash(string? filePath)
    {
        bool isHeicFile = filePath?.EndsWith(".heic", StringComparison.OrdinalIgnoreCase) ?? false;

        Bitmap? image = isHeicFile ? BitmapHelper.LoadBitmapFromPath(filePath!) : new Bitmap(filePath!);

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

        return hash.ToString();
    }

    public static string CalculateMD5Hash(byte[] imageBytes)
    {
        byte[] hashBytes = MD5.HashData(imageBytes);
        string md5Hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        return md5Hash;
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
