namespace PhotoManager.Benchmarks;

internal static class Shared
{
    private static readonly string[] JpgFileNames =
    [
        "Image 1.jpg",
        "Image 1_90_deg.jpg",
        "Image 1_180_deg.jpg",
        "Image 1_270_deg.jpg"
    ];

    private static string FindTestFilesDirectory()
    {
        string? directory = AppContext.BaseDirectory;

        while (directory != null)
        {
            string testFilesPath = Path.Combine(
                directory,
                "PhotoManager",
                "PhotoManager.Tests",
                "TestFiles");

            if (Directory.Exists(testFilesPath))
            {
                return testFilesPath;
            }

            directory = Path.GetDirectoryName(directory);
        }

        throw new DirectoryNotFoundException("Could not find TestFiles directory.");
    }

    internal static byte[][] LoadJpgImageBuffers()
    {
        string testFilesDir = FindTestFilesDirectory();

        return [.. JpgFileNames.Select(f => File.ReadAllBytes(Path.Combine(testFilesDir, f)))];
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
