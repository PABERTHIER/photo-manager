namespace PhotoManager.Tests;

public static class PathHelper
{
    /// <summary>
    /// Converts a Windows-style absolute test path (e.g. "E:\Workspace\PhotoManager\TestAssets") into an
    /// absolute path valid on the current OS.
    /// On Windows, the path is returned unchanged.
    /// On Linux and macOS, a drive-letter path is not rooted, so UserConfigurationService would resolve it
    /// against the current directory; stripping the drive and using forward slashes (e.g.
    /// "/Workspace/PhotoManager/TestAssets") keeps the "absolute path is returned unchanged" contract
    /// testable on every OS.
    /// </summary>
    /// <param name="windowsAbsolutePath">The Windows-style absolute path used by the test data.</param>
    /// <returns>An absolute path valid on the current OS.</returns>
    public static string ToPlatformAbsolutePath(string windowsAbsolutePath)
    {
        if (OperatingSystem.IsWindows())
        {
            return windowsAbsolutePath;
        }

        string withoutDrive = windowsAbsolutePath is [_, ':', ..] ? windowsAbsolutePath[2..] : windowsAbsolutePath;

        return withoutDrive.Replace('\\', '/');
    }

    /// <summary>
    /// Computes the path that UserConfigurationService returns for a Windows-style path read from the
    /// static test appsettings.json.
    /// On Windows, "E:\..." is already rooted and comes back unchanged.
    /// On Linux and macOS, a drive-letter path is not rooted, so the service resolves it against the
    /// current directory through Path.GetFullPath; the expectation must mirror that resolution.
    /// </summary>
    /// <param name="windowsAbsolutePath">The Windows-style absolute path stored in appsettings.json.</param>
    /// <returns>The path expected back from UserConfigurationService on the current OS.</returns>
    public static string ToResolvedConfigPath(string windowsAbsolutePath) =>
        OperatingSystem.IsWindows() ? windowsAbsolutePath : Path.GetFullPath(windowsAbsolutePath);
}
