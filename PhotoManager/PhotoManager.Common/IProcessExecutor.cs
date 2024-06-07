namespace PhotoManager.Common;

public interface IProcessExecutor
{
    void ExecuteFFmpegCommand(string ffmpegPath, string arguments);
}
