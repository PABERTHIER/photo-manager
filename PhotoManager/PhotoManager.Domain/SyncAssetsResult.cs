namespace PhotoManager.Domain;

public class SyncAssetsResult
{
    public required string SourceDirectory { get; set; }
    public required string DestinationDirectory { get; set; }
    public int SyncedImages { get; set; }
    public string? Message { get; set; }
}
