using System.Diagnostics;

namespace PhotoManager.Common;

public class ProcessExecutor : IProcessExecutor
{
    // TODO: When using the dll, need to know if success or not to return this bool
    public void ExecuteFFmpegCommand(string ffmpegPath, string arguments)
    {
        using (Process process = new())
        {
            process.StartInfo.FileName = ffmpegPath;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            // Event handler for output and error data
            process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
            process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Wait for the process to exit with the specified timeout
            bool processExited = process.WaitForExit(1000);

            // If the process has not exited, it means it has timed out; terminate the process forcefully
            if (!processExited)
            {
                process.Kill();
                Console.WriteLine("FFmpeg process terminated forcefully due to timeout.");
            }

            // Dispose the process
            process.Dispose();
        }
    }
}
