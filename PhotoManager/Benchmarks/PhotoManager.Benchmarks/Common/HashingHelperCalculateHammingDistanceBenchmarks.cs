using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace PhotoManager.Benchmarks.Common;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class HashingHelperCalculateHammingDistanceBenchmarks
{
    private string _hash1Sha512 = null!;
    private string _hash2Sha512 = null!;
    private string _hash1DHash = null!;
    private string _hash2DHash = null!;

    [GlobalSetup]
    public void Setup()
    {
        _hash1Sha512 =
            "a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3d4";
        _hash2Sha512 =
            "a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3d4e5f6a7b8c900e1f2a3b400d6e7f8a9b0c1d2e3f4a500c7d8e9f0a1b2c3d4";

        _hash1DHash = "0123456789abcd";
        _hash2DHash = "0123456780abce";
    }

    [Benchmark(Baseline = true)]
    [Arguments("sha512")]
    [Arguments("dhash")]
    public int Original(string hashType)
    {
        (string hash1, string hash2) = GetHashes(hashType);
        return CalculateHammingDistance_Original(hash1, hash2);
    }

    [Benchmark]
    [Arguments("sha512")]
    [Arguments("dhash")]
    public int Optimized_Span(string hashType)
    {
        (string hash1, string hash2) = GetHashes(hashType);
        return CalculateHammingDistance_Span(hash1, hash2);
    }

    [Benchmark]
    [Arguments("sha512")]
    [Arguments("dhash")]
    public int Optimized_Vectorized(string hashType)
    {
        (string hash1, string hash2) = GetHashes(hashType);
        return CalculateHammingDistance_Vectorized(hash1, hash2);
    }

    [Benchmark]
    [Arguments("sha512")]
    [Arguments("dhash")]
    public int Optimized_TensorPrimitivesHammingDistance(string hashType)
    {
        (string hash1, string hash2) = GetHashes(hashType);
        return TensorPrimitivesHammingDistance(hash1, hash2);
    }

    [Benchmark]
    [Arguments("sha512")]
    [Arguments("dhash")]
    public int Optimized_Unrolled(string hashType)
    {
        (string hash1, string hash2) = GetHashes(hashType);
        return CalculateHammingDistance_Unrolled(hash1, hash2);
    }

    private static int CalculateHammingDistance_Original(string hash1, string hash2)
    {
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

    private static int CalculateHammingDistance_Span(string hash1, string hash2)
    {
        ReadOnlySpan<char> span1 = hash1.AsSpan();
        ReadOnlySpan<char> span2 = hash2.AsSpan();

        int distance = 0;

        for (int i = 0; i < span1.Length; i++)
        {
            if (span1[i] != span2[i])
            {
                distance++;
            }
        }

        return distance;
    }

    private static int TensorPrimitivesHammingDistance(string hash1, string hash2)
    {
        return TensorPrimitives.HammingDistance(hash1.AsSpan(), hash2.AsSpan());
    }

    private static int CalculateHammingDistance_Vectorized(string hash1, string hash2)
    {
        ReadOnlySpan<char> span1 = hash1.AsSpan();
        ReadOnlySpan<char> span2 = hash2.AsSpan();
        int length = span1.Length;
        int distance = 0;
        int i = 0;

        if (Vector256.IsHardwareAccelerated && length >= Vector256<ushort>.Count)
        {
            ReadOnlySpan<ushort> ushortSpan1 = MemoryMarshal.Cast<char, ushort>(span1);
            ReadOnlySpan<ushort> ushortSpan2 = MemoryMarshal.Cast<char, ushort>(span2);

            for (; i <= length - Vector256<ushort>.Count; i += Vector256<ushort>.Count)
            {
                Vector256<ushort> v1 = Vector256.Create(ushortSpan1.Slice(i, Vector256<ushort>.Count));
                Vector256<ushort> v2 = Vector256.Create(ushortSpan2.Slice(i, Vector256<ushort>.Count));
                Vector256<ushort> cmp = Vector256.Equals(v1, v2);
                uint mask = cmp.ExtractMostSignificantBits();
                distance += BitOperations.PopCount(~mask & ((1u << Vector256<ushort>.Count) - 1));
            }
        }
        else if (Vector128.IsHardwareAccelerated && length >= Vector128<ushort>.Count)
        {
            ReadOnlySpan<ushort> ushortSpan1 = MemoryMarshal.Cast<char, ushort>(span1);
            ReadOnlySpan<ushort> ushortSpan2 = MemoryMarshal.Cast<char, ushort>(span2);

            for (; i <= length - Vector128<ushort>.Count; i += Vector128<ushort>.Count)
            {
                Vector128<ushort> v1 = Vector128.Create(ushortSpan1.Slice(i, Vector128<ushort>.Count));
                Vector128<ushort> v2 = Vector128.Create(ushortSpan2.Slice(i, Vector128<ushort>.Count));
                Vector128<ushort> cmp = Vector128.Equals(v1, v2);
                uint mask = cmp.ExtractMostSignificantBits();
                distance += BitOperations.PopCount(~mask & ((1u << Vector128<ushort>.Count) - 1));
            }
        }

        for (; i < length; i++)
        {
            if (span1[i] != span2[i])
            {
                distance++;
            }
        }

        return distance;
    }

    private static int CalculateHammingDistance_Unrolled(string hash1, string hash2)
    {
        ReadOnlySpan<char> span1 = hash1.AsSpan();
        ReadOnlySpan<char> span2 = hash2.AsSpan();
        int length = span1.Length;
        int distance = 0;
        int i = 0;

        int limit = length - 3;

        for (; i < limit; i += 4)
        {
            if (span1[i] != span2[i])
            {
                distance++;
            }
            if (span1[i + 1] != span2[i + 1])
            {
                distance++;
            }
            if (span1[i + 2] != span2[i + 2])
            {
                distance++;
            }
            if (span1[i + 3] != span2[i + 3])
            {
                distance++;
            }
        }

        for (; i < length; i++)
        {
            if (span1[i] != span2[i])
            {
                distance++;
            }
        }

        return distance;
    }

    private (string, string) GetHashes(string hashType) => hashType switch
    {
        "sha512" => (_hash1Sha512, _hash2Sha512),
        "dhash" => (_hash1DHash, _hash2DHash),
        _ => (_hash1Sha512, _hash2Sha512)
    };
}
