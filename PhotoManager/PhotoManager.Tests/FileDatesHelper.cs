namespace PhotoManager.Tests;

public static class FileDatesHelper
{
    /// <summary>
    /// Computes the file creation date that tests should expect, depending on the OS.
    /// On Windows, copying a file resets its creation date, so test files always report the day they were
    /// deployed or copied, which is the current day.
    /// On Linux, there is no API to set a file creation date: .NET synthesizes it as the older of the last
    /// metadata change (ctime) and the last modification (mtime), so it reads as the modification date
    /// whenever that date is in the past.
    /// On macOS, the kernel lowers the file birth time to the modification time whenever the modification
    /// time is set to an earlier value, leading to the same observable date as on Linux.
    /// </summary>
    /// <param name="expectedModificationDateTime">The modification date expected for the file.</param>
    /// <returns>The creation date expected for the file on the current OS.</returns>
    public static DateTime GetExpectedCreationDate(DateTime expectedModificationDateTime)
    {
        DateTime actualDate = DateTime.Now.Date;

        if (OperatingSystem.IsWindows())
        {
            return actualDate;
        }

        return expectedModificationDateTime.Date < actualDate ? expectedModificationDateTime.Date : actualDate;
    }
}
