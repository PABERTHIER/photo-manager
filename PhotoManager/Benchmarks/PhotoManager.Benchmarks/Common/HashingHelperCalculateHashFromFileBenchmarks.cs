using System.Security.Cryptography;

namespace PhotoManager.Benchmarks.Common;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[ShortRunJob]
public class HashingHelperCalculateHashFromFileBenchmarks
{
    private string _filePath = null!;
    private string _tempDirectory = null!;

    [Params(64 * 1024, 10 * 1024 * 1024)] public int FileSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"{nameof(HashingHelperCalculateHashFromFileBenchmarks)}");
        Directory.CreateDirectory(_tempDirectory);
        _filePath = Path.Combine(_tempDirectory, $"{FileSize}.bin");

        byte[] data = new byte[FileSize];
        new Random(FileSize).NextBytes(data);
        File.WriteAllBytes(_filePath, data);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Benchmark(Baseline = true)]
    public string Current_ReadAllBytes()
    {
        byte[] data = File.ReadAllBytes(_filePath);
        return HashingHelper.CalculateHash(data);
    }

    [Benchmark]
    public string Optimized_HashDataStream()
    {
        return CalculateHashFromFile(_filePath);
    }

    private static string CalculateHashFromFile(string filePath)
    {
        Span<byte> hash = stackalloc byte[SHA512.HashSizeInBytes];

        using (FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                   bufferSize: 81920, FileOptions.SequentialScan))
        {
            SHA512.HashData(stream, hash);
        }

        return ToLowerHex(hash);
    }

    private static string ToLowerHex(ReadOnlySpan<byte> hash)
    {
        return string.Create(hash.Length * 2, hash, static (chars, hashBytes) =>
        {
            for (int i = 0; i < hashBytes.Length; i++)
            {
                byte b = hashBytes[i];
                chars[i * 2] = GetHexChar(b >> 4);
                chars[(i * 2) + 1] = GetHexChar(b & 0xF);
            }
        });
    }

    private static char GetHexChar(int value) => (char)(value < 10 ? '0' + value : 'a' + value - 10);
}
