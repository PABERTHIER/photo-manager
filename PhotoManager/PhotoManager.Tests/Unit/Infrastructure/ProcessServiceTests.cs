using NUnit.Framework;
using PhotoManager.Infrastructure;
using System.Diagnostics;

namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class ProcessServiceTests
{
    [Test]
    public void IsAlreadyRunning_NoOtherProcesses_ReturnsFalse()
    {
        var processService = new ProcessService();

        bool result = processService.IsAlreadyRunning(Environment.ProcessId);

        Assert.IsFalse(result);
    }

    [Test]
    public void IsAlreadyRunning_OtherProcesses_ReturnsTrue()
    {
        var processService = new ProcessService();
        var processes = Process.GetProcesses();
        int processId;
        var fontdrvhostProcessId = processes?.FirstOrDefault(p => p.ProcessName == "fontdrvhost")?.Id; // A Windows process
        var svchostProcessId = processes?.FirstOrDefault(p => p.ProcessName == "svchost")?.Id; // A Windows DLL process

        if (fontdrvhostProcessId == null)
        {
            processId = svchostProcessId ?? 0;
        }
        else
        {
            processId = fontdrvhostProcessId ?? 0;
        }

        bool result = processService.IsAlreadyRunning(processId);

        Assert.IsTrue(result);
    }
}
