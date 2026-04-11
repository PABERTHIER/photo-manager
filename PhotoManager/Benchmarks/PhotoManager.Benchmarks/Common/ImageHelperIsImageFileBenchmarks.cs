using System.Collections.Frozen;

namespace PhotoManager.Benchmarks.Common;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ImageHelperIsImageFileBenchmarks
{
    private static readonly FrozenSet<string> ImageExtensionsFrozen = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase)
    {
        ".bmp", ".dng", ".gif", ".heic", ".ico", ".jfif",
        ".jpeg", ".jpg", ".png", ".tiff", ".tif", ".webp"
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> ImageExtensionsHashSet = new(
        StringComparer.OrdinalIgnoreCase)
    {
        ".bmp", ".dng", ".gif", ".heic", ".ico", ".jfif",
        ".jpeg", ".jpg", ".png", ".tiff", ".tif", ".webp"
    };

    private string[] _fileNames = null!;

    [GlobalSetup]
    public void Setup()
    {
        _fileNames =
        [
            "photo.JPG", "image.png", "document.pdf", "video.mp4",
            "pic.HEIC", "scan.tiff", "icon.ICO", "animation.gif",
            "raw.DNG", "photo.webp", "file.txt", "IMAGE_WITH_UPPERCASE_NAME.JPG",
            "no_extension", "photo.jfif", "image.BMP", "photo.jpeg"
        ];
    }

    [Benchmark(Baseline = true)]
    public int Original()
    {
        int count = 0;

        foreach (string fileName in _fileNames)
        {
            if (IsImageFile_Original(fileName))
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
            if (IsImageFile_FrozenSet(fileName))
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
            if (IsImageFile_HashSet(fileName))
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
            if (IsImageFile_Span(fileName))
            {
                count++;
            }
        }

        return count;
    }

    private static bool IsImageFile_Original(string fileName)
    {
        return fileName.ToLowerInvariant() switch
        {
            { } s when s.EndsWith(".bmp") => true,
            { } s when s.EndsWith(".dng") => true,
            { } s when s.EndsWith(".gif") => true,
            { } s when s.EndsWith(".heic") => true,
            { } s when s.EndsWith(".ico") => true,
            { } s when s.EndsWith(".jfif") => true,
            { } s when s.EndsWith(".jpeg") => true,
            { } s when s.EndsWith(".jpg") => true,
            { } s when s.EndsWith(".png") => true,
            { } s when s.EndsWith(".tiff") => true,
            { } s when s.EndsWith(".tif") => true,
            { } s when s.EndsWith(".webp") => true,
            _ => false
        };
    }

    private static bool IsImageFile_FrozenSet(string fileName)
    {
        string extension = Path.GetExtension(fileName);
        return ImageExtensionsFrozen.Contains(extension);
    }

    private static bool IsImageFile_HashSet(string fileName)
    {
        string extension = Path.GetExtension(fileName);
        return ImageExtensionsHashSet.Contains(extension);
    }

    private static bool IsImageFile_Span(string fileName)
    {
        ReadOnlySpan<char> extension = Path.GetExtension(fileName.AsSpan());

        if (extension.IsEmpty)
        {
            return false;
        }

        return extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".dng", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".gif", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".heic", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".ico", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".jfif", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".tiff", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".tif", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".webp", StringComparison.OrdinalIgnoreCase);
    }
}
