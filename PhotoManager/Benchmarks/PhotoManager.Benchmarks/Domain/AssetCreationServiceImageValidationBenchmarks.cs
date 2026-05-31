namespace PhotoManager.Benchmarks.Domain;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class AssetCreationServiceImageValidationBenchmarks
{
    private byte[] _imageBytes = [];

    [GlobalSetup]
    public void Setup()
    {
        _imageBytes = File.ReadAllBytes(Path.Combine(FindTestFilesDirectory(), "Image 1.jpg"));
    }

    [Benchmark(Baseline = true)]
    public byte[] Current_ValidateThenDecode()
    {
        if (!ExifHelper.IsValidImage(_imageBytes, NullLogger.Instance))
        {
            return [];
        }

        return DecodeAndCreateThumbnail();
    }

    [Benchmark]
    public byte[] Optimized_DecodeWithoutPreValidation()
    {
        return DecodeAndCreateThumbnail();
    }

    private byte[] DecodeAndCreateThumbnail()
    {
        ushort exifOrientation = ExifHelper.GetExifOrientation(_imageBytes, defaultExifOrientation: 1,
            corruptedImageOrientation: 0, NullLogger.Instance);
        ImageRotation rotation = ExifHelper.GetImageRotation(exifOrientation);
        (int originalWidth, int originalHeight) = BitmapHelper.GetImageDimensions(_imageBytes, rotation,
            NullLogger.Instance);
        (int thumbnailWidth, int thumbnailHeight) = GetThumbnailDimensions(originalWidth, originalHeight);

        using (SkiaImageData thumbnail = BitmapHelper.LoadBitmapThumbnailImage(_imageBytes, rotation, thumbnailWidth,
                   thumbnailHeight, NullLogger.Instance))
        {
            return BitmapHelper.GetJpegBitmapImage(thumbnail);
        }
    }

    private static (int Width, int Height) GetThumbnailDimensions(int originalWidth, int originalHeight)
    {
        if (originalWidth > originalHeight)
        {
            const int thumbnailMaxWidth = 100;
            float percentage = thumbnailMaxWidth * 100f / originalWidth;
            return (thumbnailMaxWidth, Convert.ToInt32(percentage * originalHeight / 100));
        }

        const int thumbnailMaxHeight = 100;
        float portraitPercentage = thumbnailMaxHeight * 100f / originalHeight;
        return (Convert.ToInt32(portraitPercentage * originalWidth / 100), thumbnailMaxHeight);
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
}
