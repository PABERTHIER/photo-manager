using System.Diagnostics;

namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class ProcessServiceTests
{
    [Test]
    public void IsAlreadyRunning_NoOtherProcesses_ReturnsFalse()
    {
        ProcessService processService = new();

        bool result = processService.IsAlreadyRunning(Environment.ProcessId);

        Assert.IsFalse(result);
    }

    [Test]
    public void IsAlreadyRunning_OtherProcesses_ReturnsTrue()
    {
        ProcessService processService = new();
        Process[] processes = Process.GetProcesses();
        int processId;
        int? fontdrvhostProcessId = processes?.FirstOrDefault(p => p.ProcessName == "fontdrvhost")?.Id; // A Windows process
        int? svchostProcessId = processes?.FirstOrDefault(p => p.ProcessName == "svchost")?.Id; // A Windows DLL process

        if (fontdrvhostProcessId == null)
        {
            processId = svchostProcessId ?? 0;
        }
        else
        {
            processId = (int)fontdrvhostProcessId;
        }

        bool result = processService.IsAlreadyRunning(processId);

        Assert.IsTrue(result);
    }
}
