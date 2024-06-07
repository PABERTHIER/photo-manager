namespace PhotoManager.Domain.Interfaces;

public interface IProcessService
{
    bool IsAlreadyRunning(int processId);
}
