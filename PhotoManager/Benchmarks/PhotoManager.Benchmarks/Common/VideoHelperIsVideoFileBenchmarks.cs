using System.Collections.Frozen;

namespace PhotoManager.Benchmarks.Common;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class VideoHelperIsVideoFileBenchmarks
{
    private static readonly FrozenSet<string> VideoExtensionsFrozen = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase)
    {
        ".3g2", ".3gp", ".asf", ".av1", ".avi", ".flv", ".m4v", ".mkv",
        ".mov", ".mp4", ".mpeg", ".mpg", ".ogv", ".webm", ".wmv"
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> VideoExtensionsHashSet = new(
        StringComparer.OrdinalIgnoreCase)
    {
        ".3g2", ".3gp", ".asf", ".av1", ".avi", ".flv", ".m4v", ".mkv",
        ".mov", ".mp4", ".mpeg", ".mpg", ".ogv", ".webm", ".wmv"
    };

    private string[] _fileNames = null!;

    [GlobalSetup]
    public void Setup()
    {
        _fileNames =
        [
            "clip.MP4", "movie.avi", "document.pdf", "photo.jpg",
            "video.MKV", "stream.webm", "record.MOV", "animation.gif",
            "film.WMV", "short.3gp", "file.txt", "CLIP_UPPERCASE.AVI",
            "no_extension", "trailer.flv", "episode.MPEG", "mobile.m4v"
        ];
    }

    [Benchmark(Baseline = true)]
    public int Original()
    {
        int count = 0;

        foreach (string fileName in _fileNames)
        {
            if (IsVideoFile_Original(fileName))
            {
                count++;
            }
        }

        return count;
    }

    [Benchmark]
    public int Optimized_FrozenSet()
    {
        int count = 0;

        foreach (string fileName in _fileNames)
        {
            if (IsVideoFile_FrozenSet(fileName))
            {
                count++;
            }
        }

        return count;
    }

    [Benchmark]
    public int Optimized_HashSet()
    {
        int count = 0;

        foreach (string fileName in _fileNames)
        {
            if (IsVideoFile_HashSet(fileName))
            {
                count++;
            }
        }

        return count;
    }

    [Benchmark]
    public int Optimized_Span()
    {
        int count = 0;

        foreach (string fileName in _fileNames)
        {
            if (IsVideoFile_Span(fileName))
            {
                count++;
            }
        }

        return count;
    }

    private static bool IsVideoFile_Original(string fileName)
    {
        return fileName.ToLowerInvariant() switch
        {
            { } s when s.EndsWith(".3g2") => true,
            { } s when s.EndsWith(".3gp") => true,
            { } s when s.EndsWith(".asf") => true,
            { } s when s.EndsWith(".av1") => true,
            { } s when s.EndsWith(".avi") => true,
            { } s when s.EndsWith(".flv") => true,
            { } s when s.EndsWith(".m4v") => true,
            { } s when s.EndsWith(".mkv") => true,
            { } s when s.EndsWith(".mov") => true,
            { } s when s.EndsWith(".mp4") => true,
            { } s when s.EndsWith(".mpeg") => true,
            { } s when s.EndsWith(".mpg") => true,
            { } s when s.EndsWith(".ogv") => true,
            { } s when s.EndsWith(".webm") => true,
            { } s when s.EndsWith(".wmv") => true,
            _ => false
        };
    }

    private static bool IsVideoFile_FrozenSet(string fileName)
    {
        string extension = Path.GetExtension(fileName);
        return VideoExtensionsFrozen.Contains(extension);
    }

    private static bool IsVideoFile_HashSet(string fileName)
    {
        string extension = Path.GetExtension(fileName);
        return VideoExtensionsHashSet.Contains(extension);
    }

    private static bool IsVideoFile_Span(string fileName)
    {
        ReadOnlySpan<char> extension = Path.GetExtension(fileName.AsSpan());

        if (extension.IsEmpty)
        {
            return false;
        }

        return extension.Equals(".3g2", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".3gp", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".asf", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".av1", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".avi", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".flv", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".m4v", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".mkv", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".mov", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".mpeg", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".mpg", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".ogv", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".webm", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".wmv", StringComparison.OrdinalIgnoreCase);
    }
}
