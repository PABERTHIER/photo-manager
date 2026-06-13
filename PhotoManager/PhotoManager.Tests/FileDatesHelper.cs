namespace PhotoManager.Tests;

public static class FileDatesHelper
{
    /// <summary>
    /// Computes the file creation date that tests should expect for a file whose whole modification history
    /// is a single date (deployed test files pinned to the fixed date, or files created during the test run).
    /// Equivalent to the two-argument overload with the same date used as origin and current modification.
    /// </summary>
    /// <param name="expectedModificationDateTime">The modification date expected for the file.</param>
    /// <returns>The creation date expected for the file on the current OS.</returns>
    public static DateTime GetExpectedCreationDate(DateTime expectedModificationDateTime) =>
        GetExpectedCreationDate(expectedModificationDateTime, expectedModificationDateTime);

    /// <summary>
    /// Computes the file creation date that tests should expect, depending on the OS.
    /// On Windows, copying a file resets its creation date, so test files always report the day they were
    /// deployed or copied, which is the current day.
    /// On Linux, there is no API to set a file creation date: .NET synthesizes it from the last metadata
    /// change (ctime) and the last modification (mtime), so it reads as the current modification date
    /// whenever that date is in the past, and as the current day after the modification date is re-set.
    /// On macOS, the kernel lowers the file birth time to the modification time whenever the modification
    /// time is set to an earlier value, but never raises it back: a file copied from a pinned test file
    /// keeps the oldest modification date it ever carried, even after its modification date is updated.
    /// </summary>
    /// <param name="originalModificationDateTime">
    /// The oldest modification date the file ever carried: the pinned date for files copied from the
    /// deployed test files, or the expected modification date for files created during the test run.
    /// </param>
    /// <param name="expectedModificationDateTime">The modification date currently expected for the file.</param>
    /// <returns>The creation date expected for the file on the current OS.</returns>
    public static DateTime GetExpectedCreationDate(DateTime originalModificationDateTime,
        DateTime expectedModificationDateTime)
    {
        DateTime actualDate = DateTime.Now.Date;

        if (OperatingSystem.IsWindows())
        {
            return actualDate;
        }

        if (OperatingSystem.IsMacOS())
        {
            DateTime oldestDate = originalModificationDateTime.Date < expectedModificationDateTime.Date
                ? originalModificationDateTime.Date
                : expectedModificationDateTime.Date;

            return oldestDate < actualDate ? oldestDate : actualDate;
        }

        return expectedModificationDateTime.Date < actualDate ? expectedModificationDateTime.Date : actualDate;
    }
}
