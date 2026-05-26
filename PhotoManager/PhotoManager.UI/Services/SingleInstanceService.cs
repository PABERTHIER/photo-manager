using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace PhotoManager.UI.Services;

public sealed class SingleInstanceService : ISingleInstanceService, IDisposable
{
    private const int UnixAccessDeniedHResult = unchecked((int)0x8007000D);
    private const int UnixResourceUnavailableHResult = unchecked((int)0x8007000B);
    private const int SharingViolationHResult = unchecked((int)0x80070020);
    private const int LockViolationHResult = unchecked((int)0x80070021);

    private readonly string _lockFilePath;

    private FileStream? _lockFileStream;

    public SingleInstanceService() : this(GetDefaultLockFilePath())
    {
    }

    public SingleInstanceService(string lockFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lockFilePath);

        _lockFilePath = lockFilePath;
    }

    public bool TryAcquire()
    {
        if (_lockFileStream is not null)
        {
            return true;
        }

        string? lockDirectory = Path.GetDirectoryName(_lockFilePath);

        if (!string.IsNullOrEmpty(lockDirectory))
        {
            Directory.CreateDirectory(lockDirectory);
        }

        try
        {
            _lockFileStream = new(_lockFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            _lockFileStream.SetLength(0);

            return true;
        }
        catch (IOException ex) when (IsLockUnavailable(ex))
        {
            return false;
        }
    }

    public static bool IsLockUnavailable(IOException exception)
    {
        if (exception.HResult is SharingViolationHResult or LockViolationHResult)
        {
            return true;
        }

        return !OperatingSystem.IsWindows()
               && exception.HResult is UnixAccessDeniedHResult or UnixResourceUnavailableHResult;
    }

    public void Dispose()
    {
        _lockFileStream?.Dispose();
        _lockFileStream = null;
    }

    [ExcludeFromCodeCoverage]
    private static string GetDefaultLockFilePath()
    {
        string applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (string.IsNullOrWhiteSpace(applicationDataPath))
        {
            applicationDataPath = Path.GetTempPath();
        }

        return Path.Combine(applicationDataPath, "PhotoManager", "PhotoManager.lock");
    }
}
