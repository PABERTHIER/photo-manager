namespace PhotoManager.UI.Models;

public delegate void MessageBoxInformationSentEventHandler(object sender, MessageBoxInformationSentEventArgs e);

public class MessageBoxInformationSentEventArgs
{
    public required string Message { get; init; }
    public required string Caption { get; init; }
}
