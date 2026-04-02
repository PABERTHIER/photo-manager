using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace PhotoManager.Benchmarks.Common;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class HashingHelperCalculateMD5HashBenchmarks
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
    public string Original(string dataName)
    {
        byte[] data = GetData(dataName);
        return CalculateMD5Hash_Original(data);
    }

    [Benchmark]
    [Arguments(nameof(_mediumData))]
    public string Optimized_ConvertToHexStringLower(string dataName)
    {
        byte[] data = GetData(dataName);
        return CalculateMD5Hash_ConvertToHexStringLower(data);
    }

    [Benchmark]
    [Arguments(nameof(_mediumData))]
    public string Optimized_StackallocStringCreate(string dataName)
    {
        byte[] data = GetData(dataName);
        return CalculateMD5Hash_StackallocStringCreate(data);
    }

    [Benchmark]
    [Arguments(nameof(_mediumData))]
    public string Optimized_TryWriteBytes(string dataName)
    {
        byte[] data = GetData(dataName);
        return CalculateMD5Hash_TryWriteBytes(data);
    }

    private static string CalculateMD5Hash_Original(byte[] imageBytes)
    {
        byte[] hashBytes = MD5.HashData(imageBytes);
        string md5Hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        return md5Hash;
    }

    private static string CalculateMD5Hash_ConvertToHexStringLower(byte[] imageBytes)
    {
        byte[] hashBytes = MD5.HashData(imageBytes);
        return Convert.ToHexStringLower(hashBytes);
    }

    private static string CalculateMD5Hash_StackallocStringCreate(byte[] imageBytes)
    {
        Span<byte> hash = stackalloc byte[MD5.HashSizeInBytes];
        MD5.HashData(imageBytes, hash);

        return string.Create(32, hash, static (chars, hashBytes) =>
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

    private static string CalculateMD5Hash_TryWriteBytes(byte[] imageBytes)
    {
        Span<byte> hash = stackalloc byte[MD5.HashSizeInBytes];
        MD5.HashData(imageBytes, hash);

        Span<char> result = stackalloc char[32];

        for (int i = 0; i < hash.Length; i++)
        {
            hash[i].TryFormat(result[(i * 2)..], out _, "x2");
        }

        return new string(result);
    }

    private byte[] GetData(string name) => name switch
    {
        nameof(_smallData) => _smallData,
        nameof(_mediumData) => _mediumData,
        nameof(_largeData) => _largeData,
        _ => _mediumData
    };
}
