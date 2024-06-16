using System.Security.AccessControl;
using System.Security.Principal;

namespace PhotoManager.Tests;

public static class DirectoryHelper
{
    public static void DenyWriteAccess(string directoryPath)
    {
        DirectoryInfo directoryInfo = new(directoryPath);
        DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();

        // Use the well-known SID for "Everyone"
        SecurityIdentifier everyone = new(WellKnownSidType.WorldSid, null);

        directorySecurity.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.WriteData, AccessControlType.Deny));

        directoryInfo.SetAccessControl(directorySecurity);
    }

    public static void AllowWriteAccess(string directoryPath)
    {
        DirectoryInfo directoryInfo = new(directoryPath);
        DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();

        // Use the well-known SID for "Everyone"
        SecurityIdentifier everyone = new(WellKnownSidType.WorldSid, null);

        directorySecurity.RemoveAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.WriteData, AccessControlType.Deny));

        directoryInfo.SetAccessControl(directorySecurity);
    }

    public static void DenyAccess(string directoryPath)
    {
        DirectoryInfo directoryInfo = new (directoryPath);
        DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();

        // Use the well-known SID for "Everyone"
        SecurityIdentifier everyone = new (WellKnownSidType.WorldSid, null);

        // Deny the "ListFolder" and "ReadData" permissions to everyone.
        // This effectively denies access to the folder.
        directorySecurity.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.ListDirectory | FileSystemRights.ReadData, AccessControlType.Deny));

        directoryInfo.SetAccessControl(directorySecurity);
    }

    public static void AllowAccess(string directoryPath)
    {
        DirectoryInfo directoryInfo = new (directoryPath);
        DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();

        // Use the well-known SID for "Everyone"
        SecurityIdentifier everyone = new (WellKnownSidType.WorldSid, null);

        // Remove the "Deny" rule for the "ListFolder" and "ReadData" permissions.
        // This effectively allows access to the folder.
        directorySecurity.RemoveAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.ListDirectory | FileSystemRights.ReadData, AccessControlType.Deny));

        directoryInfo.SetAccessControl(directorySecurity);
    }
}