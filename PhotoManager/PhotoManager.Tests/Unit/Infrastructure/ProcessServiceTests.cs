using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class ProcessServiceTests
{
    [Test]
    [Ignore("Working for Visual Studio but not for Rider")]
    public void IsAlreadyRunning_NoOtherProcesses_ReturnsFalse()
    {
        ProcessService processService = new();

        bool result = processService.IsAlreadyRunning(Environment.ProcessId);

        Assert.IsFalse(result);
    }

    [Test] // This test can fail if you have a cmd already opened
    public void IsAlreadyRunning_WithUniqueProcessName_ReturnsFalse()
    {
        ProcessService processService = new();

        // Generate a unique process name for testing
        string uniqueProcessName = Guid.NewGuid().ToString("N");

        string command;
        string arguments;
        string processName;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            command = "cmd.exe";
            arguments = $"/c echo Testing {uniqueProcessName}";
            processName = "cmd";
        }
        else
        {
            command = "/bin/bash";
            arguments = $"-c 'echo Testing {uniqueProcessName}'";
            processName = "bash";
        }

        using (Process? process = Process.Start(new ProcessStartInfo(command, arguments)))
        {
            // Wait for a short time to ensure the process has started
            Thread.Sleep(100);

            // Get the list of processes with the unique name
            Process[] processes = Process.GetProcessesByName(processName);

            // Use the ID of the first process (assuming there's only one with the unique name)
            int uniqueProcessId = processes.FirstOrDefault()?.Id ?? -1;

            bool result = processService.IsAlreadyRunning(uniqueProcessId);

            Assert.IsFalse(result);

            bool processExited = process?.WaitForExit(1000) ?? true;

            // If the process has not exited, it means it has timed out; terminate the process forcefully
            if (!processExited)
            {
                process?.Kill();
            }
        }
    }

    [Test]
    public void IsAlreadyRunning_OtherProcesses_ReturnsTrue()
    {
        ProcessService processService = new();

        int processId = -1;
        string processName;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            processName = "fontdrvhost"; // A Windows process
        }
        else
        {
            processName = "bash"; // A process name for Unix-like systems
        }

        // Find the process ID for the specified process name
        Process[] processes = Process.GetProcessesByName(processName);
        int? foundProcessId = processes?.FirstOrDefault()?.Id;

        if (foundProcessId != null)
        {
            processId = foundProcessId.Value;
        }

        // Check if the process is already running
        bool result = processService.IsAlreadyRunning(processId);

        Assert.IsTrue(result);
    }
}
