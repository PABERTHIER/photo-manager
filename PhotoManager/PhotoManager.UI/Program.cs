namespace PhotoManager.UI;

[ExcludeFromCodeCoverage]
internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception exception)
        {
            WriteStartupError(exception);
            throw;
        }
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }

    private static void WriteStartupError(Exception exception)
    {
        string applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(applicationDataPath))
        {
            applicationDataPath = Path.GetTempPath();
        }

        string logDirectory = Path.Combine(applicationDataPath, "PhotoManager");
        Directory.CreateDirectory(logDirectory);
        string logPath = Path.Combine(logDirectory, "startup-error.log");
        string message = $"{DateTime.UtcNow:O}{Environment.NewLine}{exception}{Environment.NewLine}";
        File.AppendAllText(logPath, message);
        Console.Error.WriteLine(exception);
    }
}
