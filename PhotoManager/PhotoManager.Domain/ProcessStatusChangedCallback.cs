namespace PhotoManager.Domain;

public delegate void ProcessStatusChangedCallback(ProcessStatusChangedCallbackEventArgs e);

public class ProcessStatusChangedCallbackEventArgs
{
    public required string NewStatus { get; init; }
}
