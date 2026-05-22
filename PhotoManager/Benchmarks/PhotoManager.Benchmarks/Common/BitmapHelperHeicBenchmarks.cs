using Microsoft.Extensions.Logging;

namespace PhotoManager.Benchmarks.Common;

// TODO: Rework file + clean up
internal static class BitmapHelperHeicBenchmarkHelper
{
    private static readonly string[] HeicFileNames =
    [
        "Image_11.heic",
        "Image_11_90.heic",
        "Image_11_180.heic",
        "Image_11_270.heic"
    ];

    internal static readonly ImageRotation[] Rotations =
    [
        ImageRotation.Rotate0,
        ImageRotation.Rotate90,
        ImageRotation.Rotate180,
        ImageRotation.Rotate270
    ];

    internal static readonly int[] ThumbnailWidths = [112, 200, 112, 200];
    internal static readonly int[] ThumbnailHeights = [150, 150, 150, 150];

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

    internal static byte[][] LoadImageBuffers()
    {
        string testFilesDir = FindTestFilesDirectory();

        return
        [
            .. HeicFileNames.Select(f => File.ReadAllBytes(Path.Combine(testFilesDir, f)))
        ];
    }

    internal static string[] GetImagePaths()
    {
        string testFilesDir = FindTestFilesDirectory();

        return
        [
            .. HeicFileNames.Select(f => Path.Combine(testFilesDir, f))
        ];
    }

    internal static T RunOnStaThread<T>(Func<T> func)
    {
        T result = default!;
        Exception? exception = null;

        Thread thread = new(() =>
        {
            try
            {
                result = func();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception != null)
        {
            throw exception;
        }

        return result;
    }
}

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class BitmapHelperLoadBitmapHeicOriginalImageBenchmarks
{
    private byte[][] _imageBuffers = null!;
    private ILogger _logger = null!;

    [GlobalSetup]
    public void Setup()
    {
        _imageBuffers = BitmapHelperHeicBenchmarkHelper.LoadImageBuffers();
        _logger = NullLogger<ImageProcessingService>.Instance;
    }

    [Benchmark(Baseline = true)]
    public BitmapImageData[] Original()
    {
        return BitmapHelperHeicBenchmarkHelper.RunOnStaThread(() =>
        {
            ImageRotation[] rotations = BitmapHelperHeicBenchmarkHelper.Rotations;
            BitmapImageData[] results = new BitmapImageData[_imageBuffers.Length];

            for (int i = 0; i < _imageBuffers.Length; i++)
            {
                results[i] = BitmapHelper.LoadBitmapHeicOriginalImage(_imageBuffers[i], rotations[i], _logger);
            }

            return results;
        });
    }

    [Benchmark]
    public BitmapImageData[] Optimized_StandardWpf()
    {
        return BitmapHelperHeicBenchmarkHelper.RunOnStaThread(() =>
        {
            ImageRotation[] rotations = BitmapHelperHeicBenchmarkHelper.Rotations;
            BitmapImageData[] results = new BitmapImageData[_imageBuffers.Length];

            for (int i = 0; i < _imageBuffers.Length; i++)
            {
                results[i] = BitmapHelper.LoadBitmapOriginalImage(_imageBuffers[i], rotations[i], _logger);
            }

            return results;
        });
    }
}

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class BitmapHelperLoadBitmapHeicThumbnailImageBenchmarks
{
    private byte[][] _imageBuffers = null!;
    private ILogger _logger = null!;

    [GlobalSetup]
    public void Setup()
    {
        _imageBuffers = BitmapHelperHeicBenchmarkHelper.LoadImageBuffers();
        _logger = NullLogger<ImageProcessingService>.Instance;
    }

    [Benchmark(Baseline = true)]
    public BitmapImageData[] Original()
    {
        return BitmapHelperHeicBenchmarkHelper.RunOnStaThread(() =>
        {
            ImageRotation[] rotations = BitmapHelperHeicBenchmarkHelper.Rotations;
            int[] widths = BitmapHelperHeicBenchmarkHelper.ThumbnailWidths;
            int[] heights = BitmapHelperHeicBenchmarkHelper.ThumbnailHeights;
            BitmapImageData[] results = new BitmapImageData[_imageBuffers.Length];

            for (int i = 0; i < _imageBuffers.Length; i++)
            {
                results[i] = BitmapHelper.LoadBitmapHeicThumbnailImage(_imageBuffers[i], rotations[i], widths[i],
                    heights[i], _logger);
            }

            return results;
        });
    }

    [Benchmark]
    public BitmapImageData[] Optimized_StandardWpf()
    {
        return BitmapHelperHeicBenchmarkHelper.RunOnStaThread(() =>
        {
            ImageRotation[] rotations = BitmapHelperHeicBenchmarkHelper.Rotations;
            int[] widths = BitmapHelperHeicBenchmarkHelper.ThumbnailWidths;
            int[] heights = BitmapHelperHeicBenchmarkHelper.ThumbnailHeights;
            BitmapImageData[] results = new BitmapImageData[_imageBuffers.Length];

            for (int i = 0; i < _imageBuffers.Length; i++)
            {
                results[i] = BitmapHelper.LoadBitmapThumbnailImage(_imageBuffers[i], rotations[i], widths[i],
                    heights[i], _logger);
            }

            return results;
        });
    }
}

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class BitmapHelperLoadBitmapHeicImageFromPathBenchmarks
{
    private string[] _imagePaths = null!;
    private ILogger _logger = null!;

    [GlobalSetup]
    public void Setup()
    {
        _imagePaths = BitmapHelperHeicBenchmarkHelper.GetImagePaths();
        _logger = NullLogger<ImageProcessingService>.Instance;
    }

    [Benchmark(Baseline = true)]
    public BitmapImageData[] Original()
    {
        return BitmapHelperHeicBenchmarkHelper.RunOnStaThread(() =>
        {
            ImageRotation[] rotations = BitmapHelperHeicBenchmarkHelper.Rotations;
            BitmapImageData[] results = new BitmapImageData[_imagePaths.Length];

            for (int i = 0; i < _imagePaths.Length; i++)
            {
                results[i] = BitmapHelper.LoadBitmapHeicImageFromPath(_imagePaths[i], rotations[i], _logger);
            }

            return results;
        });
    }

    [Benchmark]
    public BitmapImageData[] Optimized_StandardWpf()
    {
        return BitmapHelperHeicBenchmarkHelper.RunOnStaThread(() =>
        {
            ImageRotation[] rotations = BitmapHelperHeicBenchmarkHelper.Rotations;
            BitmapImageData[] results = new BitmapImageData[_imagePaths.Length];

            for (int i = 0; i < _imagePaths.Length; i++)
            {
                results[i] = BitmapHelper.LoadBitmapImageFromPath(_imagePaths[i], rotations[i]);
            }

            return results;
        });
    }
}
