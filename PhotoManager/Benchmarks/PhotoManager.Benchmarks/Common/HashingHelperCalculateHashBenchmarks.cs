using System.Security.Cryptography;

namespace PhotoManager.Benchmarks.Common;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class HashingHelperCalculateHashBenchmarks
{
    private byte[] _smallData = null!;
    private byte[] _mediumData = null!;
    private byte[] _largeData = null!;

    [GlobalSetup]
    public void Setup()
    {
        _smallData = new byte[10 * 1024]; // 10KB
        _mediumData = new byte[1 * 1024 * 1024]; // 1MB
        _largeData = new byte[10 * 1024 * 1024]; // 10MB

        Random.Shared.NextBytes(_smallData);
        Random.Shared.NextBytes(_mediumData);
        Random.Shared.NextBytes(_largeData);
    }

    [Benchmark(Baseline = true)]
    [Arguments(nameof(_mediumData))]
    public string Optimized_LatestVersion(string dataName)
    {
        byte[] data = GetData(dataName);
        return HashingHelper.CalculateHash(data);
    }

    [Benchmark]
    [Arguments(nameof(_mediumData))]
    public string Original(string dataName)
    {
        byte[] data = GetData(dataName);
        return CalculateHash(data);
    }

    [Benchmark]
    [Arguments(nameof(_mediumData))]
    public string Optimized_ConvertToHex(string dataName)
    {
        byte[] data = GetData(dataName);
        return CalculateHash_ConvertToHex(data);
    }

    [Benchmark]
    [Arguments(nameof(_mediumData))]
    public string Optimized_Span(string dataName)
    {
        byte[] data = GetData(dataName);
        return CalculateHash_Span(data);
    }

    [Benchmark]
    [Arguments(nameof(_mediumData))]
    public string Optimized_Stackalloc(string dataName)
    {
        byte[] data = GetData(dataName);
        return CalculateHash_Stackalloc(data);
    }

    [Benchmark]
    [Arguments(nameof(_mediumData))]
    public string Optimized_SpanFixed(string dataName)
    {
        byte[] data = GetData(dataName);
        return CalculateHash_SpanOptimized(data);
    }

    private static string CalculateHash(byte[] imageBytes)
    {
        StringBuilder hashBuilder = new();
        byte[] hash = SHA512.HashData(imageBytes);

        foreach (byte hashByte in hash)
        {
            hashBuilder.Append($"{hashByte:x2}");
        }

        return hashBuilder.ToString();
    }

    private static string CalculateHash_ConvertToHex(byte[] imageBytes)
    {
        byte[] hash = SHA512.HashData(imageBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string CalculateHash_Span(byte[] imageBytes)
    {
        Span<byte> hash = stackalloc byte[64];
        SHA512.HashData(imageBytes, hash);

        return string.Create(128, hash.ToArray(), (chars, hashBytes) =>
        {
            const string hexChars = "0123456789abcdef";
            for (int i = 0; i < hashBytes.Length; i++)
            {
                chars[i * 2] = hexChars[hashBytes[i] >> 4];
                chars[(i * 2) + 1] = hexChars[hashBytes[i] & 0xF];
            }
        });
    }

    private static string CalculateHash_Stackalloc(byte[] imageBytes)
    {
        Span<byte> hash = stackalloc byte[64];
        SHA512.HashData(imageBytes, hash);

        StringBuilder sb = new(128);
        for (int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("x2"));
        }

        return sb.ToString();
    }

    private static string CalculateHash_SpanOptimized(byte[] imageBytes)
    {
        Span<byte> hash = stackalloc byte[64];
        SHA512.HashData(imageBytes, hash);

        return string.Create(128, hash, static (chars, hashBytes) =>
        {
            const string hexChars = "0123456789abcdef";
            for (int i = 0; i < hashBytes.Length; i++)
            {
                chars[i * 2] = hexChars[hashBytes[i] >> 4];
                chars[(i * 2) + 1] = hexChars[hashBytes[i] & 0xF];
            }
        });
    }

    private byte[] GetData(string name) => name switch
    {
        nameof(_smallData) => _smallData,
        nameof(_mediumData) => _mediumData,
        nameof(_largeData) => _largeData,
        _ => _mediumData
    };
}
