using System.Diagnostics;

namespace PhotoManager.Infrastructure;

public class ProcessService : IProcessService
{
    public bool IsAlreadyRunning(int processId)
    {
        bool result = false;

        Process currentProcess = GetProcessById(processId);
        Process[] processes = GetProcessesByName(currentProcess.ProcessName);

        foreach (Process process in processes)
        {
            result = process.Id != processId;

            if (result)
            {
                break;
            }
        }

        return result;
    }

    private static Process GetProcessById(int processId)
    {
        return Process.GetProcessById(processId);
    }

    private static Process[] GetProcessesByName(string processName)
    {
        return Process.GetProcessesByName(processName);
    }
}
