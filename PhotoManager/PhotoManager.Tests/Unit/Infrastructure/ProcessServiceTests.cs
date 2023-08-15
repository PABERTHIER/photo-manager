using NUnit.Framework;
using PhotoManager.Infrastructure;

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

        bool result = processService.IsAlreadyRunning(1424); // processId 1424 is fontdrvhost

        Assert.IsTrue(result);
    }
}
