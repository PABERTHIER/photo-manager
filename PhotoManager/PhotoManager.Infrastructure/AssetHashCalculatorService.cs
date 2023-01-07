﻿using PhotoManager.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace PhotoManager.Infrastructure
{
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
    }
}
