using SkiaSharp;

namespace PhotoManager.Benchmarks.Common;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class BitmapHelperLoadBitmapImageFromPathBenchmarks
{
    private string _jpgFilePath = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        _jpgFilePath = Path.Combine(FindTestFilesDirectory(), "Image 1.jpg");
    }

    [Benchmark(Baseline = true)]
    public void Current_ReadAllBytesThenDecode()
    {
        using SkiaImageData image =
            BitmapHelper.LoadBitmapImageFromPath(_jpgFilePath, ImageRotation.Rotate0, NullLogger.Instance);
    }

    [Benchmark]
    public void Optimized_DecodeFileDirectly()
    {
        SKBitmap bitmap = SKBitmap.Decode(_jpgFilePath) ?? throw new NotSupportedException(
            "No imaging component suitable to complete this operation was found.");
        using SkiaImageData image = SkiaImageData.FromBitmapWithRotation(bitmap, ImageRotation.Rotate0);
    }

    [Benchmark]
    public void Optimized_ReadHeaderThenDecodeFileDirectly()
    {
        _ = IsHeicFormat(_jpgFilePath);
        SKBitmap bitmap = SKBitmap.Decode(_jpgFilePath) ?? throw new NotSupportedException(
            "No imaging component suitable to complete this operation was found.");
        using SkiaImageData image = SkiaImageData.FromBitmapWithRotation(bitmap, ImageRotation.Rotate0);
    }

    [Benchmark]
    public void Optimized_ReadHeaderThenDecodeStream()
    {

        using (FileStream stream = File.OpenRead(_jpgFilePath))
        {
            _ = IsHeicFormat(stream);
            stream.Position = 0;
            SKBitmap bitmap = SKBitmap.Decode(stream) ?? throw new NotSupportedException(
                "No imaging component suitable to complete this operation was found.");
            using SkiaImageData image = SkiaImageData.FromBitmapWithRotation(bitmap, ImageRotation.Rotate0);
        }
    }

    private static string FindTestFilesDirectory()
    {
        string? directory = AppContext.BaseDirectory;

        while (directory != null)
        {
            string testFilesPath = Path.Combine(directory, "PhotoManager", "PhotoManager.Tests", "TestFiles");

            if (Directory.Exists(testFilesPath))
            {
                return testFilesPath;
            }

            directory = Path.GetDirectoryName(directory);
        }

        throw new DirectoryNotFoundException("Could not find TestFiles directory.");
    }

    private static bool IsHeicFormat(string filePath)
    {
        using (FileStream stream = File.OpenRead(filePath))
        {
            return IsHeicFormat(stream);
        }
    }

    private static bool IsHeicFormat(FileStream stream)
    {
        Span<byte> header = stackalloc byte[8];
        int bytesRead = stream.Read(header);
        return bytesRead >= 8
               && header[4] == (byte)'f'
               && header[5] == (byte)'t'
               && header[6] == (byte)'y'
               && header[7] == (byte)'p';
    }
}
