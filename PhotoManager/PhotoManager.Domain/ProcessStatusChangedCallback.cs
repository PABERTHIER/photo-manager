namespace PhotoManager.Domain;

public delegate void ProcessStatusChangedCallback(ProcessStatusChangedCallbackEventArgs e);

public class ProcessStatusChangedCallbackEventArgs
{
    public string NewStatus { get; set; }
}
