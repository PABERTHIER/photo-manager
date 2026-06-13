using System.Security.AccessControl;
using System.Security.Principal;

namespace PhotoManager.Tests;

public static class DirectoryHelper
{
    public static void DenyWriteAccess(string directoryPath)
    {
        if (!OperatingSystem.IsWindows())
        {
            RemoveUnixMode(directoryPath, UnixFileMode.UserWrite | UnixFileMode.GroupWrite | UnixFileMode.OtherWrite);
            return;
        }

        DirectoryInfo directoryInfo = new(directoryPath);
        DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();

        // Use the well-known SID for "Everyone"
        SecurityIdentifier everyone = new(WellKnownSidType.WorldSid, null);

        directorySecurity.AddAccessRule(new(everyone, FileSystemRights.WriteData, AccessControlType.Deny));

        directoryInfo.SetAccessControl(directorySecurity);
    }

    public static void AllowWriteAccess(string directoryPath)
    {
        if (!OperatingSystem.IsWindows())
        {
            AddUnixMode(directoryPath, UnixFileMode.UserWrite | UnixFileMode.GroupWrite | UnixFileMode.OtherWrite);
            return;
        }

        DirectoryInfo directoryInfo = new(directoryPath);
        DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();

        // Use the well-known SID for "Everyone"
        SecurityIdentifier everyone = new(WellKnownSidType.WorldSid, null);

        directorySecurity.RemoveAccessRule(new(everyone, FileSystemRights.WriteData, AccessControlType.Deny));

        directoryInfo.SetAccessControl(directorySecurity);
    }

    public static void DenyAccess(string directoryPath)
    {
        if (!OperatingSystem.IsWindows())
        {
            RemoveUnixMode(directoryPath,
                UnixFileMode.UserRead | UnixFileMode.UserExecute
                | UnixFileMode.GroupRead | UnixFileMode.GroupExecute
                | UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
            return;
        }

        DirectoryInfo directoryInfo = new(directoryPath);
        DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();

        // Use the well-known SID for "Everyone"
        SecurityIdentifier everyone = new(WellKnownSidType.WorldSid, null);

        // Deny the "ListFolder" and "ReadData" permissions to everyone.
        // This effectively denies access to the folder.
        directorySecurity.AddAccessRule(new(everyone, FileSystemRights.ListDirectory | FileSystemRights.ReadData,
            AccessControlType.Deny));

        directoryInfo.SetAccessControl(directorySecurity);
    }

    public static void AllowAccess(string directoryPath)
    {
        if (!OperatingSystem.IsWindows())
        {
            AddUnixMode(directoryPath,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute
                | UnixFileMode.GroupRead | UnixFileMode.GroupExecute
                | UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
            return;
        }

        DirectoryInfo directoryInfo = new(directoryPath);
        DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();

        // Use the well-known SID for "Everyone"
        SecurityIdentifier everyone = new(WellKnownSidType.WorldSid, null);

        // Remove the "Deny" rule for the "ListFolder" and "ReadData" permissions.
        // This effectively allows access to the folder.
        directorySecurity.RemoveAccessRule(new(everyone, FileSystemRights.ListDirectory | FileSystemRights.ReadData,
            AccessControlType.Deny));

        directoryInfo.SetAccessControl(directorySecurity);
    }

    [System.Runtime.Versioning.UnsupportedOSPlatform("windows")]
    private static void AddUnixMode(string directoryPath, UnixFileMode mode)
    {
        File.SetUnixFileMode(directoryPath, File.GetUnixFileMode(directoryPath) | mode);
    }

    [System.Runtime.Versioning.UnsupportedOSPlatform("windows")]
    private static void RemoveUnixMode(string directoryPath, UnixFileMode mode)
    {
        File.SetUnixFileMode(directoryPath, File.GetUnixFileMode(directoryPath) & ~mode);
    }
}
